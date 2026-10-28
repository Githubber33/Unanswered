using UnityEngine;


public class SendToSerialOnStart : MonoBehaviour
{
    private SerialPortManager spManager; // Get access to the serialport defined in the SerialPortManager    
    public string messageToSerialOnStart = ""; // What message to send to the serialPort

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

        // Send message safely when port is ready
        spManager.OnPortReady += SendBeginMessage;
    }

    void SendBeginMessage()
    {
        spManager.SendSafe(messageToSerialOnStart);
        Debug.Log("Begin message sent: " + messageToSerialOnStart);
    }




}
