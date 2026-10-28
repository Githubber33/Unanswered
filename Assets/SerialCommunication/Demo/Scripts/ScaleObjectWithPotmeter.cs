using UnityEngine;

public class ScaleObjectWithPotmeter : MonoBehaviour
{
    private SerialPortManager spManager;

    [Header("Scale Settings")]
    public float minScale = 0.5f;
    public float maxScale = 3f;

    private float currentScale = 1f;

    void Start()
    {
        spManager = SerialPortManager.instance;
        spManager.OnRawDataReceived += ProcessRawData;
    }

    void ProcessRawData(string raw)
    {
        if (int.TryParse(raw, out int sensorVal))
        {
            float t = sensorVal / 1023f;
            currentScale = Mathf.Lerp(minScale, maxScale, t);
        }
    }

    void Update()
    {
        transform.localScale = new Vector3(currentScale, currentScale, currentScale);
    }

    void OnDestroy()
    {
        if (spManager != null)
            spManager.OnRawDataReceived -= ProcessRawData;
    }
}