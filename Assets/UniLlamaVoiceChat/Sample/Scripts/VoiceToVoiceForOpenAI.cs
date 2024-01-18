using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using Assets.Mochineko.WhisperAPI;
using Cysharp.Threading.Tasks;
using Mochineko.ChatGPT_API;
using Mochineko.ChatGPT_API.Memory;
using Mochineko.Relent.Resilience;
using Mochineko.Relent.UncertainResult;
using Mochineko.VoiceActivityDetection;
using UniLLMVoiceChat.StyleBertVITS2;
using UniLLMVoiceChat.Util;
using UnityEngine;

namespace UniLLMVoiceChat.Sample
{
    public class VoiceToVoiceForOpenAI : MonoBehaviour, IWaveStreamReceiver
    {
        /// <summary>
        /// System message to instruct assistant.
        /// </summary>
        [SerializeField, TextArea] private string systemMessage = string.Empty;

        /// <summary>
        /// Max number of chat memory of queue.
        /// </summary>
        [SerializeField] private int maxMemoryCount = 20;

        private ChatCompletionAPIConnection? connection;
        private IChatMemory? memory;

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
            model: Assets.Mochineko.WhisperAPI.Model.Whisper1,
            language: "ja");

        private static readonly HttpClient httpClient = new();

        private void Awake()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void Start()
        {
            // StyleBertVITS2のサーバーURLを書き換える
            StyleBertVITS2Util.StyleBertVITSBaseURL = "VOICE_SERVICE_URL";
            
            memory = new FiniteQueueChatMemory(maxMemoryCount);

            // Create instance of ChatGPTConnection with specifying chat model.
            connection = new ChatCompletionAPIConnection(
                "YOUR_OPENAI_API_KEY",
                memory,
                systemMessage);


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
            var apiKey = "YOUR OPEN API KEY";
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
                        var chatResponse = await SendChatAsync(text,cancellationToken: cancellationToken);

                        if (100 <= chatResponse.Length)
                        {
                            Debug.LogError("LLMによる推論の長さが100文字を超えています。文字数を強制的に99文字にします。");
                            chatResponse = chatResponse[..99];
                        }

                        // 返信テキストから音声を生成する
                        var param = new StyleBertVITS2RequestParameters
                        {
                            Text = chatResponse,
                            ModelId = 1
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

        private async UniTask<string> SendChatAsync(string message,CancellationToken cancellationToken)
        {
            // Validations
            if (connection == null)
            {
                Debug.LogError($"[ChatGPT_API.Samples] Connection is null.");
                return null;
            }

            if (string.IsNullOrEmpty(message))
            {
                Debug.LogError($"[ChatGPT_API.Samples] Chat content is empty.");
                return null;
            }

            ChatCompletionResponseBody response;
            try
            {
                await UniTask.SwitchToThreadPool();

                // Create message by ChatGPT chat completion API.
                response = await connection.CompleteChatAsync(
                    message,
                    cancellationToken);
            }
            catch (Exception e)
            {
                // Exceptions should be caught.
                Debug.LogException(e);
                return null;
            }

            await UniTask.SwitchToMainThread(cancellationToken);

            // Log chat completion result.
            Debug.Log($"[ChatGPT_API.Samples] Result:\n{response.ResultMessage}");
            return response.ResultMessage;
        }
    }
}