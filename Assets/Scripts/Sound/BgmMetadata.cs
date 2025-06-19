using UnityEngine;

public class BgmMetadata : MonoBehaviour
{
    [SerializeField] AudioClip bgmClip;
    [Range(0.0f, 1.0f)]
    [SerializeField] float targetVolume;
    [Header("Loop data")]
    [SerializeField] int sampleRate;
    [SerializeField] int sampleLoopStart;
    [SerializeField] int sampleLoopFinish;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public AudioClip GetBgmClip()
    {
        return bgmClip;
    }

    public float GetVolume()
    {
        return targetVolume;
    }

    public int GetSampleRate()
    {
        return sampleRate;
    }

    public int[] GetLoopSamples()
    {
        int[] samples = {sampleLoopStart, sampleLoopFinish};
        return samples;
    }
}
