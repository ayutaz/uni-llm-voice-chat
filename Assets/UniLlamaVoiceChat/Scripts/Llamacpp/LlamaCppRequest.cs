namespace UniLlamaVoiceChat.Llamacpp
{
    /// <summary>
    /// llama.cppのサーバーへのリクエストクラス
    /// </summary>
    public class LlamaCppRequest
    {
        /// <summary>
        /// テキスト
        /// </summary>
        public string prompt;

        /// <summary>
        /// 生成されるテキストのランダム性
        /// </summary>
        public float temperature = 0.8f;

        /// <summary>
        /// 次のトークンの選択を、最も可能性の高い K 個のトークンに制限
        /// </summary>
        public int top_k = 40;
        
        /// <summary>
        /// 次のトークンの選択を、累積確率がしきい値 P (デフォルト: 0.95) を超えるトークンのサブセットに制限
        /// </summary>
        public float top_p = 0.95f;

        /// <summary>
        /// 最も可能性の高いトークンの確率と比較した、考慮されるトークンの最小確率
        /// </summary>
        public float min_p = 0.05f;


        /// <summary>
        /// テキストを生成するときに予測するトークンの最大数を設定します
        /// </summary>
        public int n_predict = -1;

        /// <summary>
        /// 完了を待つのではなく、予測された各トークンをリアルタイムで受信できるようになります。
        /// </summary>
        public bool stream = false;
    }
}