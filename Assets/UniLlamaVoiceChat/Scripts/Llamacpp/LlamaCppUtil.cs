using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace UniLlamaVoiceChat.Llamacpp
{
    /// <summary>
    /// calm2-chatモデルを使用したチャット機能
    /// </summary>
    public static class LlamaCppUtil
    {
        /// <summary>
        /// llama.cppのサーバーURL
        /// </summary>
        private const string LlamaServerURL = "http://127.0.0.1:8080/completion";

        /// <summary>
        /// チャットを送信する
        /// </summary>
        /// <param name="llamaCppRequest"></param>
        /// <param name="cancellationToken"></param>
        public static async UniTask<string> PostRequest(LlamaCppRequest llamaCppRequest,CancellationToken cancellationToken)
        {
            // JSONデータの準備
            var jsonData = JsonUtility.ToJson(llamaCppRequest);

            // UnityWebRequestオブジェクトの作成
            using var webRequest = new UnityWebRequest(LlamaServerURL, "POST");
            // リクエストボディの設定
            var jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
            webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            // ヘッダーの設定
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // リクエストを送信し、完了を待つ
            await webRequest.SendWebRequest().ToUniTask(cancellationToken:cancellationToken).SuppressCancellationThrow();

            // エラーチェック
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error: " + webRequest.error);
                return null;
            }

            // レスポンスの表示
            var response = JsonUtility.FromJson<LlamaCppResponse>(webRequest.downloadHandler.text);
            return response.content;
        }
    }
}