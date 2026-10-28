using UnityEngine;


public class SendToSerialOnCollision : MonoBehaviour
{
    private SerialPortManager spManager; // Get access to the serialport defined in the SerialPortManager
    public string messageToSerial = ""; // What message to send to the serialPort



    // Start is called before the first frame update
    void Start()
    {
        spManager = SerialPortManager.instance;  // Obtain the serial port from the manager                                              

        if (spManager == null)
        {
            Debug.LogError("SerialPortManager not found!");
            enabled = false;
            return;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Simply call SendSafe, manager handles cooldown automatically
        spManager.SendSafe(messageToSerial);
    }




}
