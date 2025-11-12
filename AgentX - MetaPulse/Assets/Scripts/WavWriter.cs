// WavWriter.cs
using System;
using System.IO;
using UnityEngine;

public static class WavWriter
{
    // Convert float samples (-1..1) to PCM16 little-endian
    public static byte[] FromFloatArray(float[] samples, int sampleRate, int channels, bool forceMono = true)
    {
        // Downmix to mono if needed
        float[] mono = samples;
        int outChannels = channels;
        if (forceMono && channels > 1)
        {
            int frames = samples.Length / channels;
            mono = new float[frames];
            for (int i = 0; i < frames; i++)
            {
                float sum = 0f;
                for (int c = 0; c < channels; c++) sum += samples[i * channels + c];
                mono[i] = sum / channels;
            }
            outChannels = 1;
        }

        // Convert to Int16
        short[] pcm = new short[mono.Length];
        for (int i = 0; i < mono.Length; i++)
        {
            float f = Mathf.Clamp(mono[i], -1f, 1f);
            pcm[i] = (short)Mathf.RoundToInt(f * short.MaxValue);
        }

        // Build WAV into a MemoryStream
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        int byteRate = sampleRate * outChannels * 2;      // 16-bit = 2 bytes
        int dataSize = pcm.Length * 2;
        int riffSize = 36 + dataSize;

        // RIFF header
        bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        bw.Write(riffSize);
        bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

        // fmt  chunk (PCM)
        bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        bw.Write(16);                  // PCM chunk size
        bw.Write((short)1);            // AudioFormat = 1 (PCM)
        bw.Write((short)outChannels);  // NumChannels
        bw.Write(sampleRate);          // SampleRate
        bw.Write(byteRate);            // ByteRate
        bw.Write((short)(outChannels * 2)); // BlockAlign
        bw.Write((short)16);           // BitsPerSample

        // data chunk
        bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        bw.Write(dataSize);

        // Samples
        foreach (short s in pcm) bw.Write(s);

        bw.Flush();
        return ms.ToArray();
    }

    // Helper: encode an entire AudioClip
    public static byte[] FromAudioClip(AudioClip clip, bool forceMono = true)
    {
        int channels = clip.channels;
        int samples = clip.samples * channels;
        float[] buffer = new float[samples];
        clip.GetData(buffer, 0);
        return FromFloatArray(buffer, clip.frequency, channels, forceMono);
    }
}
