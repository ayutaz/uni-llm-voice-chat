using System.Threading;
using UniLLMVoiceChat.Llamacpp;
using UniLLMVoiceChat.StyleBertVITS2;
using UniLLMVoiceChat.Util;
using UnityEngine;

namespace UniLLMVoiceChat.Sample
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
                    
                    const string instruction = "以下について回答してください";
                    var prompt = 
                        "以下は、タスクを説明する指示と、文脈のある入力の組み合わせです。要求を適切に満たす応答を書きなさい。\n\n" +
                        "### 指示:\n" +
                        instruction + "\n\n" +
                        "### 入力:\n" +
                        textInputToVoiceView.InputMessage + "\n\n" +
                        "### 応答:\n";
                    
                    // llama.cppサーバーに対するリクエストパラメーターを作成する
                    var requestParam = new LlamaCppRequest
                    {
                        prompt = prompt,
                        temperature = 0.8f,
                        n_predict =30,
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