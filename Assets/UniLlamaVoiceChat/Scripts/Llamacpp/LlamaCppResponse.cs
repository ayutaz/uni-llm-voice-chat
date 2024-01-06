using System.Collections.Generic;

namespace UniLlamaVoiceChat.Llamacpp
{
    /// <summary>
    /// llama.cppのサーバーからのレスポンス
    /// </summary>
    public class LlamaCppResponse
    {
        /// <summary>
        /// 生成されたテキスト
        /// </summary>
        public string content;
        
        /// <summary>
        /// オプションの設定
        /// </summary>
        public GenerationSettings generation_settings;
        
        /// <summary>
        /// ロードされたモデルのパス
        /// </summary>
        public string model;
        
        /// <summary>
        /// プロンプト
        /// </summary>
        public string prompt;
        
        /// <summary>
        /// 完了タスクを特定のスロットに割り当てる。
        /// </summary>
        public int slot_id;
        public bool stop;
        public bool stopped_eos;
        public bool stopped_limit;
        public bool stopped_word;
        public string stopping_word;
        
        /// <summary>
        /// 完了に関するタイミング情報のハッシュ。
        /// </summary>
        public Timings timings;
        
        /// <summary>
        /// 前回の完了から再利用できるプロンプトのトークンの数 (n_past)
        /// </summary>
        public int tokens_cached;
        
        /// <summary>
        /// プロンプトから評価されたトークンの総数
        /// </summary>
        public int tokens_evaluated;
        public int tokens_predicted;
        public bool truncated;
    }

    /// <summary>
    /// オプションの設定
    /// </summary>
    public class GenerationSettings
    {
        public float frequency_penalty;
        public string grammar;
        public bool ignore_eos;
        public List<object> logit_bias;
        public float min_p;
        public int mirostat;
        public float mirostat_eta;
        public float mirostat_tau;
        public string model;
        public int n_ctx;
        public int n_keep;
        public int n_predict;
        public int n_probs;
        public bool penalize_nl;
        public List<object> penalty_prompt_tokens;
        public float presence_penalty;
        public int repeat_last_n;
        public float repeat_penalty;
        public ulong seed;
        public List<object> stop;
        public bool stream;
        public float temperature;
        public float tfs_z;
        public int top_k;
        public float top_p;
        public float typical_p;
        public bool use_penalty_prompt_tokens;
    }

    /// <summary>
    /// 完了に関するタイミング情報のハッシュ。
    /// </summary>
    public class Timings
    {
        public double predicted_ms;
        public int predicted_n;
        public double predicted_per_second;
        public double predicted_per_token_ms;
        public double prompt_ms;
        public int prompt_n;
        public double prompt_per_second;
        public double prompt_per_token_ms;
    }
}