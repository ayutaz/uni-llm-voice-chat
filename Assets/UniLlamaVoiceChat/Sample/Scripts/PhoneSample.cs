using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.VoiceActivityDetection;
using UniLLMVoiceChat.Llamacpp;
using UniLLMVoiceChat.StyleBertVITS2;
using UniLLMVoiceChat.Util;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace UniLLMVoiceChat.Sample
{
    /// <summary>
    /// PhoneSample
    /// </summary>
    public class PhoneSample : MonoBehaviour
    {
        /// <summary>
        /// 音声データのキュー
        /// </summary>
        private readonly Queue<byte[]> _voiceDataQueue = new Queue<byte[]>();

        [SerializeField] private VADParameters parameters;

        [SerializeField] private AudioSource audioSource;

        private UnityMicrophoneProxy? proxy;
        private IVoiceActivityDetector? vad;

        private CancellationTokenSource _cancellationTokenSource;

        // APIエンドポイントのURLを設定
        private const string URL = "http://127.0.0.1:8000/transcribe";

        private void Awake()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            StyleBertVITS2Util.StyleBertVITSBaseURL = "https://4e94-203-137-131-72.ngrok-free.app/";
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

            vad = new QueueingVoiceActivityDetector(
                source,
                audioClipBuffer,
                parameters.MaxQueueingTimeSeconds,
                parameters.MinQueueingTimeSeconds,
                parameters.ActiveVolumeThreshold,
                parameters.ActivationRateThreshold,
                parameters.InactivationRateThreshold,
                parameters.ActivationIntervalSeconds,
                parameters.InactivationIntervalSeconds,
                parameters.MaxActiveDurationSeconds);

            audioClipBuffer.OnVoiceInactive
                .Subscribe(clip =>
                {
                    Debug.Log("[VAD.Samples] OnInactive and receive AudioClip and play.");
                    var audioBytes = clip.ToWav();
                    if (audioBytes == null) throw new NullReferenceException(nameof(audioBytes));
                    _voiceDataQueue.Enqueue(audioBytes);
                    
                    // 自分の声を再生する
                    // audioSource.clip = clip;
                    // audioSource.Play();
                }).AddTo(this);
        }

        private void Update()
        {
            vad?.Update();

            if (!_voiceDataQueue.TryDequeue(out var stream)) return;
            Debug.Log("[VAD.Samples] Dequeue wave stream.");

            TranscribeAsync(stream, this.GetCancellationTokenOnDestroy())
                .Forget();
        }

        private static async UniTask<string> SendTranscriptionRequest(byte[] wavBytes,
            CancellationToken cancellationToken)
        {
            // multipart/form-data形式のフォームデータを準備
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("file", wavBytes, "audio.wav", "audio/wav")
            };

            // UnityWebRequest を初期化し、POSTリクエストを設定
            using var request = UnityWebRequest.Post(URL, formData);
            request.downloadHandler = new DownloadHandlerBuffer();

            try
            {
                // リクエストを送信
                await request.SendWebRequest().WithCancellation(cancellationToken);

                return request.result == UnityWebRequest.Result.Success
                    ?
                    // 成功時の処理
                    request.downloadHandler.text
                    :
                    // リトライ可能な失敗の処理
                    request.error;
            }
            catch (OperationCanceledException)
            {
                // キャンセル時の処理
                return null;
            }
        }


        private async UniTask TranscribeAsync(byte[] wavBytes, CancellationToken cancellationToken)
        {
            Debug.Log($"[VAD.Samples] Begin to transcribe for audio stream: {wavBytes.Length} bytes.");

            // faster-whisper サーバーに対してリクエストを送る
            var transcribeResult = await SendTranscriptionRequest(wavBytes, cancellationToken);

            if (transcribeResult != null)
            {
                await HandleSuccess(transcribeResult);
            }
            else
            {
                Debug.LogError("[VAD.Samples] Transcribed text is empty.");
            }
        }

        private async UniTask HandleSuccess(string success)
        {
            // var text = TranscriptionResponseBody.FromJson(success)?.Text;
            if (success is { Length: > 0 })
            {
                // FIXME: Log.Debug is not working at this.
                Debug.LogFormat("[VAD.Samples] Succeeded to transcribe into: {0}.", success);

                var prompt =
                    $"USER: あなたはつくよみちゃんです。あなたの名前はなんですか？\n " +
                    "ASSISTANT: 私の名前はつくよみちゃんです!\n" +
                    "USER: 好きな食べ物はなんですか？\n" +
                    "ASSISTANT: 絵に描いた餅だよ\n" +
                    $"USER: {success}\n"+
                    "ASSISTANT:";


                // llama.cppサーバーに対するリクエストパラメーターを作成する
                var requestParam = new LlamaCppRequest
                {
                    prompt = prompt,
                    temperature = 0.8f,
                    n_predict = 30,
                };
                var chatResponse = await LlamaCppUtil.PostRequest(requestParam,
                    _cancellationTokenSource.Token);

                if (100 <= chatResponse.Length)
                {
                    Debug.LogError($"LLMによる推論の長さが100文字を超えています。[{chatResponse}]");
                    return;
                }

                // 返信テキストから音声を生成する
                var param = new StyleBertVITS2RequestParameters
                {
                    Text = chatResponse,
                    ModelId = 0
                };
                Debug.Log("llm response: " + chatResponse);
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
        }

        private void OnDestroy()
        {
            vad?.Dispose();
            proxy?.Dispose();
        }
    }
}