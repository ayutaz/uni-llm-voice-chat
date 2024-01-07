# キャラクターとの音声対話プロジェクト
[llama.cpp](https://github.com/ggerganov/llama.cpp)をLLMサーバーとして推論を行います。また推論内容から音声合成ライブラリ(*1)を用いて音声を生成するライブラリです。   
サンプルシーンもあるため、cloneしてそのまま実行することができます。   
またライブラリとしてもご活用いただけます。

(*1) 対応している音声合成ライブラリ
- [x] [Style-Bert-VITS2](https://github.com/litagin02/Style-Bert-VITS2)
- [ ] [Bert-VITS2](https://github.com/fishaudio/Bert-VITS2)
- [ ] [fish-speech](https://github.com/fishaudio/fish-speech)
- [ ] [Koemotion](https://rinna.co.jp/products/business/koemotion/)
- [ ] [VOICEVOX](https://voicevox.hiroshiba.jp/)
- [ ] [COEIROINK](https://coeiroink.com/)


<!-- TOC -->
* [キャラクターとの音声対話プロジェクト](#キャラクターとの音声対話プロジェクト)
* [usage](#usage)
  * [llama.cppのセットアップ](#llamacppのセットアップ)
  * [Style-Bert-VITS2のセットアップ](#style-bert-vits2のセットアップ)
  * [Unityのセットアップ](#unityのセットアップ)
* [ライブラリとしてインストール](#ライブラリとしてインストール)
  * [UPM](#upm)
  * [Unity Package](#unity-package)
* [requirements](#requirements)
* [using library for Sample](#using-library-for-sample)
* [License](#license)
<!-- TOC -->

# usage
## llama.cppのセットアップ
1. [llama.cpp](https://github.com/ggerganov/llama.cpp)をcloneします。
2. `llama.cpp`のディレクトリに移動し、`make`(*2)を実行します。
3. `models`にモデルを配置します。
4. `llama.cpp`のディレクトリに移動し、`./server.exe -m models/{model_name}`を実行します。llama.cppのサーバーAPIの詳細は[llama.cpp/example/server](https://github.com/ggerganov/llama.cpp/blob/master/examples/server/README.md)を参照してください。

(*2)
makeを実行するためには、プラットフォームごとに異なる手順が必要です。

* Linux or MacOS

  `make`を実行します

* Windows

  [w64devkit](https://github.com/skeeto/w64devkit/releases)をインストールした上で、`make` を実行します。

詳しくは[llama.cpp](https://github.com/ggerganov/llama.cpp?tab=readme-ov-file#build)を参照してください。

## Style-Bert-VITS2のセットアップ
1. [Style-Bert-VITS2](https://github.com/litagin02/Style-Bert-VITS2)をcloneします。
2. 必要なライブラリをインストールします。詳細は、[インストール](https://github.com/litagin02/Style-Bert-VITS2?tab=readme-ov-file#%E3%82%A4%E3%83%B3%E3%82%B9%E3%83%88%E3%83%BC%E3%83%AB)を参照してください。
3. `model_assets`にモデルを配置します。このときに必要なファイルは、`config.json` と `*.safetensors` と `style_vectors.npy` が必要です。
4. `python server_fastapi.py` を実行します。APIの詳細は実行後に表示されるURLを参照してください。

## Unityのセットアップ
1. Unityでプロジェクトを開きます。
2. `Assets/Scenes/`にある`Sample`を開きます。

# ライブラリとしてインストール
## UPM
1. パッケージマネージャーを開く
2. 左上にある`+`ボタンをクリックする
3. 「`git URLからパッケージを追加...`」を選択します。
4. https://github.com/ayutaz/ayutaz/uni-llama-voice-chat.git?path=Assets/UniLlamaVoiceChat/Scripts` のURLを追加する。
5. `Add`をクリックします。

## Unity Package
1. [リリースページ](https://github.com/ayutaz/uni-llama-voice-chat/releases)から最新リリースをダウンロードする。
2. パッケージをプロジェクトにインポートする

# requirements
* Unity 2023.2.4f1
* [UniTask](https://github.com/Cysharp/UniTask)

# using library for Sample
* [UniTask](https://github.com/Cysharp/UniTask)
* Font: [Noto Sans Japanese](https://fonts.google.com/noto/specimen/Noto+Sans+JP?subset=japanese&noto.script=Hira)

# License
* [Apache License 2.0](https://github.com/ayutaz/uni-llama-voice-chat/blob/main/LICENSE)
* Font: [Open Font License](https://openfontlicense.org/)