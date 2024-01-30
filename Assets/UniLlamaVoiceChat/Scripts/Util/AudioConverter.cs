using System;
using System.IO;
using UnityEngine;

namespace UniLLMVoiceChat.Util
{
    /// <summary>
    /// WAVデータをAudioClipに変換するクラス
    /// </summary>
    public static class AudioConverter
    {
        /// <summary>
        /// WAVのビットレート
        /// </summary>
        private enum Resolution
        {
            _16bit = 16,
            _24bit = 24,
            _32bit = 32
        }

        /// <summary>
        /// WAVのbyte[]データをAudioClipに変換する
        /// </summary>
        /// <param name="wavFile"></param>
        /// <param name="stream"></param>
        public static AudioClip ConvertByteToAudioClip(byte[] wavFile, bool stream = false)
        {
            // WAVファイルかどうかをチェック
            if (!IsWAVFile(wavFile))
            {
                return null;
            }

            var subchunk1Size = BitConverter.ToInt32(wavFile, 16);
            int audioFormat = BitConverter.ToInt16(wavFile, 20);
            int numChannels = BitConverter.ToInt16(wavFile, 22);
            var sampleRate = BitConverter.ToInt32(wavFile, 24);
            var bitsPerSample = (Resolution)BitConverter.ToInt16(wavFile, 34);
            // PCM WAV method:
            if (audioFormat != 1)
            {
                return null;
            }

            // Find where data starts:
            var dataIndex = 20 + subchunk1Size;
            for (var i = dataIndex; i < wavFile.Length; i++)
            {
                if (wavFile[i] == 'd' && wavFile[i + 1] == 'a' && wavFile[i + 2] == 't' && wavFile[i + 3] == 'a')
                {
                    dataIndex = i + 4; // "data" string size = 4
                    break;
                }
            }

            // Data parameters:
            var subchunk2Size = BitConverter.ToInt32(wavFile, dataIndex); // Data size (Subchunk2Size).
            dataIndex += 4; // Subchunk2Size = 4
            var sampleSize = (int)bitsPerSample / 8; // Size of a sample.
            var sampleCount = subchunk2Size / sampleSize; // How many samples into data.
            // Data conversion:
            var audioBuffer = new float[sampleCount]; // Size for all available channels.
            for (var i = 0; i < sampleCount; i++)
            {
                var sampleIndex = dataIndex + i * sampleSize;
                var intSample = 0;
                float sample = 0;
                switch (bitsPerSample)
                {
                    case Resolution._16bit:
                        intSample = BitConverter.ToInt16(wavFile, sampleIndex);
                        sample = intSample / 32767f;
                        break;
                    case Resolution._24bit:
                        intSample = BitConverter.ToInt32(
                            new byte[]
                            {
                                0, wavFile[sampleIndex], wavFile[sampleIndex + 1], wavFile[sampleIndex + 2]
                            }, 0) >> 8;
                        sample = intSample / 8388607f;
                        break;
                    case Resolution._32bit:
                        intSample = BitConverter.ToInt32(wavFile, sampleIndex);
                        sample = intSample / 2147483647f;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                audioBuffer[i] = sample;
            }

            // Create the AudioClip:
            var audioClip =
                AudioClip.Create(string.Empty, sampleCount / numChannels, numChannels, sampleRate, stream);
            audioClip.SetData(audioBuffer, 0);
            return audioClip;
        }

        /// <summary>
        /// byte配列データがWAVファイルかどうかを判定する
        /// </summary>
        /// <returns></returns>
        private static bool IsWAVFile(byte[] data)
        {
            // WAVファイルの最小長チェック
            if (data == null || data.Length < 44)
            {
                return false;
            }

            // "RIFF"ヘッダのチェック
            var riffHeader = System.Text.Encoding.ASCII.GetString(data, 0, 4);
            if (riffHeader != "RIFF")
            {
                return false;
            }

            // "WAVE"フォーマットのチェック
            var waveFormat = System.Text.Encoding.ASCII.GetString(data, 8, 4);
            if (waveFormat != "WAVE")
            {
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// AudioClipをWAVのbyte[]に変換する
        /// </summary>
        /// <param name="audioClip"></param>
        /// <returns></returns>
        public static byte[] ToWav(this AudioClip audioClip)
        {
            // AudioClipからデータを取得
            var samples = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(samples, 0);

            // WAVファイルのバイト配列を作成
            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);
            // WAVヘッダーの書き込み
            WriteWavHeader(writer, audioClip, samples.Length);

            // サンプルデータの書き込み
            foreach (var sample in samples)
            {
                var intValue = (short)(sample * short.MaxValue);
                writer.Write(intValue);
            }

            return memoryStream.ToArray();
        }

        private static void WriteWavHeader(BinaryWriter writer, AudioClip clip, int sampleCount)
        {
            writer.Write(System.Text.Encoding.UTF8.GetBytes("RIFF")); // ChunkID
            writer.Write(36 + sampleCount * 2); // ChunkSize
            writer.Write(System.Text.Encoding.UTF8.GetBytes("WAVE")); // Format
            writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt ")); // Subchunk1ID
            writer.Write(16); // Subchunk1Size
            writer.Write((short)1); // AudioFormat
            writer.Write((short)clip.channels); // NumChannels
            writer.Write(clip.frequency); // SampleRate
            writer.Write(clip.frequency * clip.channels * 2); // ByteRate
            writer.Write((short)(clip.channels * 2)); // BlockAlign
            writer.Write((short)16); // BitsPerSample
            writer.Write(System.Text.Encoding.UTF8.GetBytes("data")); // Subchunk2ID
            writer.Write(sampleCount * 2); // Subchunk2Size
        }
    }
}