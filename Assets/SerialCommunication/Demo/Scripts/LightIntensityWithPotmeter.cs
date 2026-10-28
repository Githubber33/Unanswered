using UnityEngine;

public class LightIntensityWithPotmeter : MonoBehaviour
{
    private SerialPortManager spManager;
    private Light sceneLight;

    public float minIntensity = 0f;
    public float maxIntensity = 5f;

    void Start()
    {
        sceneLight = GetComponent<Light>();
        spManager = SerialPortManager.instance;
        spManager.OnRawDataReceived += ProcessRawData;
    }

    void ProcessRawData(string raw)
    {
        if (int.TryParse(raw, out int sensorVal))
        {
            float t = sensorVal / 1023f;
            sceneLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
        }
    }

    void OnDestroy()
    {
        if (spManager != null)
            spManager.OnRawDataReceived -= ProcessRawData;
    }
}