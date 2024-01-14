using System.Threading;
using System.Web;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace UniLLMVoiceChat.StyleBertVITS2
{
    /// <summary>
    /// StyleBertVITS2モデルを使用する際のユーティリティクラス
    /// </summary>
    public static class StyleBertVITS2Util
    {
        /// <summary>
        /// StyleBertVITS2のサーバーURL
        /// </summary>
        private const string StyleBertVITSBaseURL = "http://127.0.0.1:5000/";

        /// <summary>
        /// テキストを音声に変換する
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async UniTask<byte[]> TextToSpeechAsync(StyleBertVITS2RequestParameters parameters,CancellationToken cancellationToken)
        {
            var url = $"{StyleBertVITSBaseURL}voice?{ToQueryString(parameters)}";

            using var request = UnityWebRequest.Get(url);
            // リクエストを送信し、レスポンスを待つ
            await request.SendWebRequest().WithCancellation(cancellationToken).SuppressCancellationThrow();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {request.error}");
                return null;
            }

            // WAVデータを取得
            return request.downloadHandler.data;
        }
        
        // URLクエリ文字列を生成するメソッド
        private static string ToQueryString(StyleBertVITS2RequestParameters styleBertVits2RequestParameters)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["text"] = styleBertVits2RequestParameters.Text;
            query["encoding"] = styleBertVits2RequestParameters.Encoding;
            query["model_id"] = styleBertVits2RequestParameters.ModelId.ToString();
            query["speaker_id"] = styleBertVits2RequestParameters.SpeakerId.ToString();
            
            if (!string.IsNullOrEmpty(styleBertVits2RequestParameters.SpeakerName))
            {
                query["speaker_name"] = styleBertVits2RequestParameters.SpeakerName;
            }
            query["sdp_ratio"] = styleBertVits2RequestParameters.SdpRatio.ToString();
            query["noise"] = styleBertVits2RequestParameters.Noise.ToString();
            query["noisew"] = styleBertVits2RequestParameters.Noisew.ToString();
            query["length"] = styleBertVits2RequestParameters.Length.ToString();
            query["language"] = styleBertVits2RequestParameters.Language;
            query["auto_split"] = styleBertVits2RequestParameters.AutoSplit.ToString();
            query["split_interval"] = styleBertVits2RequestParameters.SplitInterval.ToString();
            query["assist_text"] = styleBertVits2RequestParameters.AssistText;
            query["assist_text_weight"] = styleBertVits2RequestParameters.AssistTextWeight.ToString();
            query["style"] = styleBertVits2RequestParameters.Style;
            query["style_weight"] = styleBertVits2RequestParameters.StyleWeight.ToString();
            query["reference_audio_path"] = styleBertVits2RequestParameters.ReferenceAudioPath;

            // query.ToString() は自動的にURLエンコードされたクエリ文字列を返す
            return query.ToString();
        }
    }
}