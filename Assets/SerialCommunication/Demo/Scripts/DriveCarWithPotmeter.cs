using System.Collections;
using UnityEngine;



public class DriveCarWithPotmeter : MonoBehaviour
{

    private SerialPortManager spManager; // Get access to the serialport defined in the SerialPortManager

    public float turnSpeed = 10.0f;

    [Header("The drivespeed (mappedValue) will be manage by the potentialmeter")]
    [SerializeField, Tooltip("Raw value of Arduino, only for debug")]

    public float mappedValue;
    //public float speed = 0f;

    float verticalInput;

    float horizontalInput;

    // Start is called before the first frame update
    void Start()
    {
        spManager = SerialPortManager.instance;

        // Subscriben on new data
        spManager.OnRawDataReceived += ProcessRawData;

    }
    void Update()
    {
        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up * Time.deltaTime * turnSpeed * horizontalInput);

        transform.Translate(Vector3.forward * Time.deltaTime * mappedValue * verticalInput);

    }
    void ProcessRawData(string raw)
    {
        if (int.TryParse(raw, out int sensorVal))
        {
            // translate 0-1023 to 0–100 
            mappedValue = Mathf.Lerp(0f, 100f, sensorVal / 1023f);
            // Debug.Log("Raw sensor value: " + sensorVal);
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
