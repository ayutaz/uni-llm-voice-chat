using TMPro;
using UnityEngine;

namespace UniLLMVoiceChat.Sample
{
    /// <summary>
    /// VoiceInputToVoiceResponseView
    /// </summary>
    public class VoiceInputToVoiceResponseView : MonoBehaviour
    {
        /// <summary>
        /// チャットの履歴表示
        /// </summary>
        [SerializeField] private TextMeshProUGUI chatText;
        
        /// <summary>
        /// チャットの履歴にテキストを追加する
        /// </summary>
        /// <param name="text"></param>
        public void AddChatText(string text)
        {
            chatText.text += $"{text}\n";
        }
    }
}