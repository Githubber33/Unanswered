using UnityEngine;

public class AudioVolumeWithPotmeter : MonoBehaviour
{
    private SerialPortManager spManager;
    private AudioSource audioSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float minVolume = 0f;

    [Range(0f, 1f)]
    public float maxVolume = 1f;

    private float currentVolume;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        spManager = SerialPortManager.instance;
        spManager.OnRawDataReceived += ProcessRawData;
    }

    void ProcessRawData(string raw)
    {
        if (int.TryParse(raw, out int sensorVal))
        {
            float t = sensorVal / 1023f;

            currentVolume = Mathf.Lerp(minVolume, maxVolume, t);
        }
    }

    void Update()
    {
        audioSource.volume = currentVolume;
    }

    void OnDestroy()
    {
        if (spManager != null)
            spManager.OnRawDataReceived -= ProcessRawData;
    }
}