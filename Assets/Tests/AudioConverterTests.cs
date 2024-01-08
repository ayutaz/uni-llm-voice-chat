using NUnit.Framework;
using UniLlamaVoiceChat.Util;

namespace Tests
{
    public class AudioConverterTests
    {
        [Test]
        public void ConvertByteToAudioClipWithInvalidWavReturnsNull()
        {
            // 不適切なWAVファイルデータを用意
            var invalidWavData = new byte[] { 0, 1, 2, 3, 4, 5 }; // 不正なデータ

            // ConvertByteToAudioClipを実行
            var result = AudioConverter.ConvertByteToAudioClip(invalidWavData);

            // 結果がnullであることを確認
            Assert.IsNull(result);
        }
    }
}