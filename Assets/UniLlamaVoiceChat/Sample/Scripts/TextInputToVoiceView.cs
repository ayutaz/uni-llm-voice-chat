using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UniLLMVoiceChat.Sample
{
    /// <summary>
    /// TextChatToVoiceのViewクラス
    /// </summary>
    public class TextInputToVoiceView : MonoBehaviour
    {
        /// <summary>
        /// チャットの入力フィールド
        /// </summary>
        [SerializeField] private TMP_InputField inputMessageField;
        
        /// <summary>
        /// チャットの送信ボタン
        /// </summary>
        [SerializeField] private Button sendButton;
        
        /// <summary>
        /// チャットの履歴表示
        /// </summary>
        [SerializeField] private TextMeshProUGUI chatText;
        
        /// <summary>
        /// チャットの入力フィールドのテキスト
        /// </summary>
        public string InputMessage => inputMessageField.text;
        
        /// <summary>
        /// チャットの送信ボタンが押された時のイベント
        /// </summary>
        public Button OnClickSendButton => sendButton;

        /// <summary>
        /// チャットの履歴にテキストを追加する
        /// </summary>
        /// <param name="text"></param>
        public void AddChatText(string text)
        {
            chatText.text += $"{text}\n";
        }
        
        /// <summary>
        /// チャットの入力フィールドをクリアする
        /// </summary>
        public void ClearInputMessage()
        {
            inputMessageField.text = string.Empty;
        }
    }
}