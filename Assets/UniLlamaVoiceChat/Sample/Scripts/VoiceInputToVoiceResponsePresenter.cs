using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using Assets.Mochineko.WhisperAPI;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Resilience;
using Mochineko.Relent.UncertainResult;
using Mochineko.VoiceActivityDetection;
using UniLLMVoiceChat.Llamacpp;
using UniLLMVoiceChat.StyleBertVITS2;
using UniLLMVoiceChat.Util;
using UnityEngine;

namespace UniLLMVoiceChat.Sample
{
    /// <summary>
    /// VoiceInputToVoiceResponsePresenter
    /// </summary>
    public class VoiceInputToVoiceResponsePresenter : MonoBehaviour, IWaveStreamReceiver
    {
        [SerializeField] private VADParameters parameters;

        [SerializeField] private AudioSource audioSource;

        /// <summary>
        /// 会話履歴画面
        /// </summary>
        [SerializeField] private VoiceInputToVoiceResponseView _voiceResponseView;

        private UnityMicrophoneProxy? proxy;
        private IVoiceActivityDetector? vad;

        private CancellationTokenSource _cancellationTokenSource;

        private readonly Queue<Stream> streamQueue = new();
        private readonly IPolicy<string> policy = WhisperPolicyFactory.Build();

        private readonly TranscriptionRequestParameters requestParameters = new(
            file: "UnityMicVAD.wav",
            model: Model.Whisper1,
            language: "ja");

        private static readonly HttpClient httpClient = new();

        private void Awake()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void Start()
        {
            if (parameters == null)
            {
                throw new NullReferenceException(nameof(parameters));
            }

            if (audioSource == null)
            {
                throw new NullReferenceException(nameof(audioSource));
            }

            proxy = new UnityMicrophoneProxy();

            IVoiceSource source = new UnityMicrophoneSource(proxy);

            var audioClipBuffer = new AudioClipBuffer(
                maxSampleLength: (int)(parameters.MaxActiveDurationSeconds * source.SamplingRate),
                frequency: source.SamplingRate);

            var buffer = new CompositeVoiceBuffer(
                new WaveVoiceBuffer(this), // To wave file and Whisper transcription API.
                audioClipBuffer // To AudioClip and AudioSource (echo debug).
            );

            vad = new QueueingVoiceActivityDetector(
                source,
                buffer,
                parameters.MaxQueueingTimeSeconds,
                parameters.MinQueueingTimeSeconds,
                parameters.ActiveVolumeThreshold,
                parameters.ActivationRateThreshold,
                parameters.InactivationRateThreshold,
                parameters.ActivationIntervalSeconds,
                parameters.InactivationIntervalSeconds,
                parameters.MaxActiveDurationSeconds);
        }

        private void Update()
        {
            vad?.Update();

            if (streamQueue.TryDequeue(out var stream))
            {
                Debug.Log("[VAD.Samples] Dequeue wave stream.");

                TranscribeAsync(stream, this.GetCancellationTokenOnDestroy())
                    .Forget();
            }
        }

        void IWaveStreamReceiver.OnReceive(Stream stream)
        {
            Debug.Log("[VAD.Samples] OnReceive wave stream.");

            streamQueue.Enqueue(stream);
        }

        private async UniTask TranscribeAsync(Stream stream, CancellationToken cancellationToken)
        {
            // API key must be set in environment variable.
            var apiKey = "YOUR_OPEN_API_KEY";
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new NullReferenceException(nameof(apiKey));
            }

            Debug.Log($"[VAD.Samples] Begin to transcribe for audio stream: {stream.Length} bytes.");

            // Dispose stream when out of scope.
            await using var _ = stream;

            // Transcribe speech into text by Whisper transcription API.
            var result = await policy
                .ExecuteAsync(async innerCancellationToken
                        => await TranscriptionAPI
                            .TranscribeAsync(
                                apiKey,
                                httpClient,
                                stream,
                                requestParameters,
                                innerCancellationToken,
                                debug: false),
                    cancellationToken);

            switch (result)
            {
                // Success
                case IUncertainSuccessResult<string> success:
                {
                    var text = TranscriptionResponseBody.FromJson(success.Result)?.Text;
                    if (text is { Length: > 0 })
                    {
                        // FIXME: Log.Debug is not working at this.
                        Debug.LogFormat("[VAD.Samples] Succeeded to transcribe into: {0}.", text);
                        _voiceResponseView.AddChatText(text);

                        const string instruction = "以下について回答してください";
                        var prompt = 
                            "以下は、タスクを説明する指示と、文脈のある入力の組み合わせです。要求を適切に満たす応答を書きなさい。\n\n" +
                            "### 指示:\n" +
                            instruction + "\n\n" +
                            "### 入力:\n" +
                            text + "\n\n" +
                            "### 応答:\n";


                        // llama.cppサーバーに対するリクエストパラメーターを作成する
                        var requestParam = new LlamaCppRequest
                        {
                            prompt = prompt,
                            temperature = 0.8f,
                            n_predict = 30,
                        };
                        var chatResponse = await LlamaCppUtil.PostRequest(requestParam,
                            _cancellationTokenSource.Token);
                        _voiceResponseView.AddChatText(chatResponse);

                        if (100 <= chatResponse.Length)
                        {
                            Debug.LogError("LLMによる推論の長さが100文字を超えています。");
                            return;
                        }

                        // 返信テキストから音声を生成する
                        var param = new StyleBertVITS2RequestParameters
                        {
                            Text = chatResponse
                        };
                        var audioResponse =
                            await StyleBertVITS2Util.TextToSpeechAsync(param, _cancellationTokenSource.Token);
                        var audioClip = AudioConverter.ConvertByteToAudioClip(audioResponse);
                        audioSource.clip = audioClip;
                        audioSource.Play();
                    }
                    else
                    {
                        Debug.LogWarning("[VAD.Samples] Transcribed text is empty.");
                    }

                    break;
                }
                // Retryable failure
                case IUncertainRetryableResult<string> retryable:
                {
                    // FIXME: Log.Error is not working at this.
                    Debug.LogErrorFormat("[VAD.Samples] Retryable failed to transcribe because -> {0}.",
                        retryable.Message);
                    break;
                }
                // Failure
                case IUncertainFailureResult<string> failure:
                {
                    // FIXME: Log.Error is not working at this.
                    Debug.LogErrorFormat("[VAD.Samples] Failed to transcribe because -> {0}.", failure.Message);
                    break;
                }
                default:
                    throw new UncertainResultPatternMatchException(nameof(result));
            }
        }

        private void OnDestroy()
        {
            vad?.Dispose();
            proxy?.Dispose();
        }
    }
}