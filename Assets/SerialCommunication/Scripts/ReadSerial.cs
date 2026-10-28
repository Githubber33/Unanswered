using UnityEngine;



public class ReadSerial : MonoBehaviour
{
    private SerialPortManager spManager;  // Get access to the serialport defined in the SerialPortManager
    public float mappedValue; // the value after remapping the value received through the serialport 

    void Start()
    {
        spManager = SerialPortManager.instance;

        // Subscriben on new data
        spManager.OnRawDataReceived += ProcessRawData;
    }

    void ProcessRawData(string raw)
    {
        if (int.TryParse(raw, out int sensorVal))
        {
            // example translate 0-1023 to 0–100 
            mappedValue = Mathf.Lerp(0f, 100f, sensorVal / 1023f);
            //Debug.Log(sensorVal);
        }
        else
        {
            Debug.LogWarning("Could not parse raw value: '" + raw + "'");
        }
    }

    void OnDestroy()
    {
        if (spManager != null)
            spManager.OnRawDataReceived -= ProcessRawData;
    }

}
