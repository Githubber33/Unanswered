using UnityEngine;

public class RotateObjectWithPotmeter : MonoBehaviour
{
    private SerialPortManager spManager;
    private float rotationY;

    public float minRotation = 0f;
    public float maxRotation = 360f;

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
            rotationY = Mathf.Lerp(minRotation, maxRotation, t);
        }
    }

    void Update()
    {
        transform.rotation = Quaternion.Euler(0, rotationY, 0);
    }

    void OnDestroy()
    {
        if (spManager != null)
            spManager.OnRawDataReceived -= ProcessRawData;
    }
}