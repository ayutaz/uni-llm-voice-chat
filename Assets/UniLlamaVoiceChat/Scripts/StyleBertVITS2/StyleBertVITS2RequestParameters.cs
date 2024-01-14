namespace UniLLMVoiceChat.StyleBertVITS2
{
    /// <summary>
    /// テキストを音声に変換する際のパラメーター
    /// </summary>
    public class StyleBertVITS2RequestParameters
    {
        /// <summary>
        /// セリフ
        /// </summary>
        public string Text;
        
        /// <summary>
        /// エンコード
        /// </summary>
        public string Encoding = "utf-8";
        
        /// <summary>
        ///  モデルID.GET /models/infoのkeyの値を指定する
        /// </summary>
        public int ModelId = 0;
        
        /// <summary>
        /// 話者ID。model_assets>[model]>config.json内のspk2idを確認
        /// </summary>
        public int SpeakerId = 0;
        
        /// <summary>
        /// 話者名(speaker_idより優先)。esd.listの2列目の文字列を指定
        /// </summary>
        public string SpeakerName;
        
        /// <summary>
        /// SDP(Stochastic Duration Predictor)/DP混合比。比率が高くなるほどトーンのばらつきが大きくなる
        /// </summary>
        public float SdpRatio = 0.2f;
        
        /// <summary>
        /// サンプルノイズの割合。大きくするほどランダム性が高まる
        /// </summary>
        public float Noise = 0.6f;
        
        /// <summary>
        /// SDPノイズ。大きくするほど発音の間隔にばらつきが出やすくなる
        /// </summary>
        public float Noisew = 0.8f;
        
        /// <summary>
        /// 話速。基準は1で大きくするほど音声は長くなり読み上げが遅まる
        /// </summary>
        public float Length = 1;
        
        /// <summary>
        /// テキストの言語
        /// </summary>
        public string Language = "JP";
        
        /// <summary>
        /// 改行で分けて生成するかどうか
        /// </summary>
        public bool AutoSplit = true;
        
        /// <summary>
        /// 分けた場合に挟む無音の長さ（秒
        /// </summary>
        public float SplitInterval = 0.5f;
        
        /// <summary>
        /// このテキストの読み上げと似た声音・感情になりやすくなる。ただし抑揚やテンポ等が犠牲になる傾向がある
        /// </summary>
        public string AssistText = string.Empty;
        
        /// <summary>
        /// assist_textの強さ
        /// </summary>
        public float AssistTextWeight = 1;
        
        /// <summary>
        /// スタイル
        /// </summary>
        public string Style = "Neutral";
        
        /// <summary>
        /// スタイルの強さ
        /// </summary>
        public float StyleWeight = 5;
        
        /// <summary>
        /// スタイルを音声ファイルで行う
        /// </summary>
        public string ReferenceAudioPath;
    }
}