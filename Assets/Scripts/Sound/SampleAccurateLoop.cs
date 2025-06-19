using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SampleAccurateLoop : MonoBehaviour
{
    [Header("Sample rates")]
    public int fileSampleRate;
    public int targetSampleRate = 48000;

    [Header("Loop Settings")]
    public int loopStartSample; // Start of loop in samples
    public int loopEndSample;   // End of loop in samples


    private AudioSource audioSource;
    private float[] originalAudioData;
    private float[] resampledAudioData;
    private int numChannels;
    private int currentSample = 0;
    private int adjustedLoopStart;
    private int adjustedLoopEnd;
    private float volume;
    private bool isPlaying;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        isPlaying = false;
    }

    public void SetPlaying(bool playing)
    {
        isPlaying = playing;
    }

    public void ResetPosition()
    {
        currentSample = 0;
    }

    public void SetClipSource(AudioClip clip)
    {
        audioSource.clip = clip;
    }

    public void SetSampleInfo(int sampleRate, int loopStart, int loopEnd)
    {
        fileSampleRate = sampleRate;
        loopStartSample = loopStart;
        loopEndSample = loopEnd;
    }

    /// <summary>
    /// Resamples the clip and adjusts the loop points to the target sample rate
    /// </summary>
    public void PrepareClip()
    {
        numChannels = audioSource.clip.channels;

        adjustedLoopStart = Mathf.RoundToInt(loopStartSample * ((float)targetSampleRate / fileSampleRate));
        adjustedLoopEnd = Mathf.RoundToInt(loopEndSample * ((float)targetSampleRate / fileSampleRate));

        // Load original audio data
        originalAudioData = new float[audioSource.clip.samples * numChannels];
        audioSource.clip.GetData(originalAudioData, 0);

        // Resample the clip to target sample rate
        resampledAudioData = ResampleAudio(originalAudioData, audioSource.clip.frequency, targetSampleRate, numChannels);

        // Replace the clip with resampled audio
        AudioClip resampledClip = AudioClip.Create("ResampledClip", resampledAudioData.Length / numChannels, numChannels, targetSampleRate, false);
        resampledClip.SetData(resampledAudioData, 0);

        volume = audioSource.volume;

        audioSource.clip = resampledClip;
    }

    private float[] ResampleAudio(float[] source, int sourceRate, int targetRate, int channels)
    {
        float ratio = (float)sourceRate / targetRate;
        int newSampleCount = Mathf.CeilToInt(source.Length / ratio);
        float[] resampled = new float[newSampleCount];

        for (int i = 0; i < newSampleCount / channels; i++)
        {
            int oldIndex = Mathf.FloorToInt(i * ratio) * channels;
            for (int channel = 0; channel < channels; channel++)
            {
                if (oldIndex + channel < source.Length)
                    resampled[i * channels + channel] = source[oldIndex + channel];
            }
        }

        return resampled;
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (!isPlaying || resampledAudioData == null || resampledAudioData.Length == 0)
        {
            // Clear buffer when stopped
            for (int i = 0; i < data.Length; i++)
                data[i] = 0;
            return;
        }

        int bufferSamples = data.Length / channels;

        for (int i = 0; i < bufferSamples; i++)
        {
            if (currentSample >= adjustedLoopEnd)
            {
                currentSample = adjustedLoopStart;
            }

            int sampleIndex = currentSample * numChannels;

            for (int channel = 0; channel < channels; channel++)
            {
                int audioDataIndex = sampleIndex + channel;

                if (audioDataIndex < resampledAudioData.Length)
                {
                    data[i * channels + channel] = resampledAudioData[audioDataIndex] * volume;
                }
                else
                {
                    data[i * channels + channel] = 0; // Prevent out-of-bounds errors
                }
            }

            currentSample++;
        }
    }
}
