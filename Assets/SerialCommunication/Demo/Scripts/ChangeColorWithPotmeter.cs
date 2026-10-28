using UnityEngine;

public class ChangeColorWithPotmeter : MonoBehaviour
{
    private SerialPortManager spManager;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        spManager = SerialPortManager.instance;
        spManager.OnRawDataReceived += ProcessRawData;
    }

    void ProcessRawData(string raw)
    {
        if (int.TryParse(raw, out int sensorVal))
        {
            float t = sensorVal / 1023f;

            Color newColor = Color.Lerp(Color.blue, Color.red, t);
            rend.material.color = newColor;
        }
    }

    void OnDestroy()
    {
        if (spManager != null)
            spManager.OnRawDataReceived -= ProcessRawData;
    }
}