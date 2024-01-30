# キャラクターとの音声対話プロジェクト

![](oss-icon.png)

LLM(*1)をサーバーとして推論を行います。また推論内容から音声合成ライブラリ(*2)を用いて音声を生成するライブラリです。   
サンプルシーンもあるため、cloneしてそのまま実行することができます。   
またライブラリとしてもご活用いただけます。

<!-- TOC -->
* [キャラクターとの音声対話プロジェクト](#キャラクターとの音声対話プロジェクト)
* [Phoneサンプルのセットアップ方法](#phoneサンプルのセットアップ方法)
  * [llama.cppのセットアップ](#llamacppのセットアップ)
  * [Style-Bert-VITS2のセットアップ](#style-bert-vits2のセットアップ)
  * [音声の文字起こし](#音声の文字起こし)
  * [Unityのセットアップ](#unityのセットアップ)
* [各種対応しているライブラリ](#各種対応しているライブラリ)
* [ライブラリとしてインストール](#ライブラリとしてインストール)
  * [UPM](#upm)
  * [Unity Package](#unity-package)
* [requirements](#requirements)
* [using library for Sample](#using-library-for-sample)
* [License](#license)
<!-- TOC -->

# Phoneサンプルのセットアップ方法

以下はVADを使い音声入力を検知しつつローカルにて、音声の文字起こし→LLMによる推論→音声合成を行うサンプルシーンのセットアップ方法です。

<details>
<summary>LLM推論(llama.cpp)のセットアップ (click to expand)</summary>

## llama.cppのセットアップ

1. [llama.cpp](https://github.com/ggerganov/llama.cpp)をcloneします。
2. `llama.cpp`のディレクトリに移動し、`make`(*3)を実行します。
3. `models`にモデルを配置します。
4. `llama.cpp`のディレクトリに移動し、`./server.exe -m models/{model_name}`
   を実行します。llama.cppのサーバーAPIの詳細は[llama.cpp/example/server](https://github.com/ggerganov/llama.cpp/blob/master/examples/server/README.md)
   を参照してください。

(*3)
makeを実行するためには、プラットフォームごとに異なる手順が必要です。

* Linux or MacOS

  `make`を実行します

* Windows

  [w64devkit](https://github.com/skeeto/w64devkit/releases)をインストールした上で、`make` を実行します。

詳しくは[llama.cpp](https://github.com/ggerganov/llama.cpp?tab=readme-ov-file#build)を参照してください。

</details>

<details>
<summary>音声合成(Style-Bert-VITS2)のセットアップ (click to expand)</summary>

## Style-Bert-VITS2のセットアップ

1. [Style-Bert-VITS2](https://github.com/litagin02/Style-Bert-VITS2)をcloneします。
2. `pip install -r requirements.txt` で必要なライブラリをインストールします。
3. `python initialize.py`で必要なモデルをダウンロードします。
4. `model_assets`にモデルを配置します。このときに必要なファイルは、`config.json` と `*.safetensors` と `style_vectors.npy`
   が必要です。
5. `python server_fastapi.py` を実行します。APIの詳細は実行後に表示されるURLを参照してください。

つくよみちゃんコーパスで学習したモデルを使用する場合は、[こちら](https://huggingface.co/ayousanz/tsukuyomi-chan-style-bert-vits2-model)からダウンロードして `model_assets`に配置してください。
</details>

<details>
<summary>音声の文字起こし(faster-whisper-server)のセットアップ (click to expand)</summary>

## 音声の文字起こし

1. [faster-whisper-server](https://github.com/ayutaz/faster-whisper-server.git)をcloneします。
2. `faster-whisper-server`のディレクトリに移動し、`docker-compose up -d`を実行します。

</details>

## Unityのセットアップ

1. Unityでプロジェクトを開きます。
2. `Assets/Scenes/`にある`PhoneSample`を開きます。

# 各種対応しているライブラリ
(*1) 対応しているLLM

- [x] [llama.cpp](https://github.com/ggerganov/llama.cpp)
- [x] [OpenAI](https://openai.com/)

(*2) 対応している音声合成ライブラリ

- [x] [Style-Bert-VITS2](https://github.com/litagin02/Style-Bert-VITS2)
- [ ] [Koemotion](https://rinna.co.jp/products/business/koemotion/)
- [ ] [VOICEVOX](https://voicevox.hiroshiba.jp/)
- [ ] [COEIROINK](https://coeiroink.com/)
- [ ] [OpenAITTS](https://platform.openai.com/docs/guides/text-to-speech)

# ライブラリとしてインストール

## UPM

1. パッケージマネージャーを開く
2. 左上にある`+`ボタンをクリックする
3. 「`git URLからパッケージを追加...`」を選択します。
4. `https://github.com/ayutaz/uni-llama-voice-chat.git?path=Assets/UniLlamaVoiceChat/Scripts` のURLを追加する。
5. `Add`をクリックします。

## Unity Package

1. [リリースページ](https://github.com/ayutaz/uni-llama-voice-chat/releases)から最新リリースをダウンロードする。
2. パッケージをプロジェクトにインポートする

# requirements

* Unity 2023.2.4f1
* [UniTask](https://github.com/Cysharp/UniTask)

# using library for Sample

* [UniTask](https://github.com/Cysharp/UniTask)
* [voice-activity-detection-unity](https://github.com/mochi-neko/voice-activity-detection-unity)
* Font: [Noto Sans Japanese](https://fonts.google.com/noto/specimen/Noto+Sans+JP?subset=japanese&noto.script=Hira)

# License

* [Apache License 2.0](https://github.com/ayutaz/uni-llama-voice-chat/blob/main/LICENSE)
* Font: [Open Font License](https://openfontlicense.org/)