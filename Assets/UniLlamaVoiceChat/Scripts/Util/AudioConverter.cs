using System;
using UnityEngine;

namespace UniLlamaVoiceChat.Util
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
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static AudioClip ConvertByteToAudioClip(byte[] wavFile, bool stream = false)
        {
            var subchunk1Size = BitConverter.ToInt32(wavFile, 16);
            int audioFormat = BitConverter.ToInt16(wavFile, 20);
            int numChannels = BitConverter.ToInt16(wavFile, 22);
            var sampleRate = BitConverter.ToInt32(wavFile, 24);
            var bitsPerSample = (Resolution)BitConverter.ToInt16(wavFile, 34);
            // PCM WAV method:
            if (audioFormat == 1)
            {
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
            
            Debug.LogError("AudioFormat is not PCM");
            return null;
        }
    }
}