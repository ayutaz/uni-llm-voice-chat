using System.Threading;
using UniLlamaVoiceChat.Llamacpp;
using UniLlamaVoiceChat.StyleBertVITS2;
using UniLlamaVoiceChat.Util;
using UnityEngine;

namespace UniLlamaVoiceChat.Sample
{
    /// <summary>
    /// ユーザーがテキストで入力したチャットをキャラクター側が音声で反応するクラス
    /// </summary>
    public class TextInputToVoicePresenter : MonoBehaviour
    {
        /// <summary>
        /// audioSource
        /// </summary>
        [SerializeField] private AudioSource audioSource;

        /// <summary>
        /// 画面
        /// </summary>
        [SerializeField] private TextInputToVoiceView textInputToVoiceView;

        private CancellationTokenSource _cancellationTokenSource;

        private void Awake()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void Start()
        {
            textInputToVoiceView.OnClickSendButton
                .onClick.AddListener(async () =>
                {
                    if (string.IsNullOrEmpty(textInputToVoiceView.InputMessage))
                    {
                        return;
                    }
                    
                    textInputToVoiceView.AddChatText(textInputToVoiceView.InputMessage);
                    
                    // llama.cppサーバーに対するリクエストパラメーターを作成する
                    var requestParam = new LlamaCppRequest
                    {
                        prompt = textInputToVoiceView.InputMessage,
                        temperature = 0.8f,
                        n_predict = 10,
                        stream = true,
                    };
                    var chatResponse = await LlamaCppUtil.PostRequest(requestParam,
                        _cancellationTokenSource.Token);
                    textInputToVoiceView.ClearInputMessage();
                    textInputToVoiceView.AddChatText(chatResponse);
                    
                    // 返信テキストから音声を生成する
                    var param = new StyleBertVITS2RequestParameters
                    {
                        Text = chatResponse
                    };
                    var audioResponse = await StyleBertVITS2Util.TextToSpeechAsync(param, _cancellationTokenSource.Token);
                    var audioClip = AudioConverter.ConvertByteToAudioClip(audioResponse);
                    audioSource.clip = audioClip;
                    audioSource.Play();
                });
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}