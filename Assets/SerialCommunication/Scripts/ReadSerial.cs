using UnityEngine;
using System.Globalization;

public class ReadSerial : MonoBehaviour
{
    private SerialPortManager spManager;

    [Header("Distance Sensor")]
    public float distanceCm = -1f;

    void Start()
    {
        spManager = SerialPortManager.instance;

        if (spManager == null)
        {
            Debug.LogError("SerialPortManager not found!");
            enabled = false;
            return;
        }

        spManager.OnRawDataReceived += ProcessRawData;
    }

    void ProcessRawData(string raw)
    {
        if (raw == "INVALID")
            return;

        if (float.TryParse(
            raw,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out float value) && value > 0f)
        {
            distanceCm = value;
        }
    }

    void OnDestroy()
    {
        if (spManager != null)
            spManager.OnRawDataReceived -= ProcessRawData;
    }
}
