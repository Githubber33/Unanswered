using System.Collections;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using System;
// =========================================================
/// Central manager for all Serial communication.
/// Handles opening the port, sending messages, and threaded reading.
/// Other scripts should never access SerialPort directly.
// =========================================================
public class SerialPortManager : MonoBehaviour

{
    // Singleton instance to ensure only one SerialPortManager exists
    public static SerialPortManager instance;

    [Header("Serial Settings")]
    [Tooltip("Enter your COM port (Windows) or /dev/... (Mac)")]
    public string port = "/dev/tty.usbserial-11240"; // making a public variable with a type tekst allows us to change the port in Unity
    public int bautrate = 9600; // making a public variable with a type a whole number, allows us to change the bautrate in Unity

    [Header("Shutdown Message")]
    [Tooltip("Message to send when application quits")]
    public string messageToSerialOnStop = ""; // message to send to the serialPort when the connection to serial stops

    private SerialPort serialPort;
    private bool isReady = false;     // Internal flag to track when the serial port is ready   
    public bool IsReady => isReady;  // Public read-only property to safely expose readiness

    // Threaded reading
    private Thread readThread;
    private volatile bool keepReading = true;

    // Thread-safe buffer
    private string threadValue;
    private bool newDataAvailable = false;
    private readonly object lockObject = new object();

    [Header("Debug - Raw Arduino Data")]
    [SerializeField, Tooltip("Raw value of Arduino, only for debug")]
    // Raw value from Arduino
    private string latestRawValue;
    public string LatestRawValue => latestRawValue;


    // Event-driven for student scripts
    public event Action<string> OnRawDataReceived;
    public event Action OnPortReady;

    // Cooldown settings
    private float sendCooldown = 0.5f;
    private float lastSentTime;

    // =========================================================

    void Awake()
    {
        // Singleton pattern, so there is only one instance
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(OpenSerialPort());
    }

    void Update()
    {
        // Move thread data safely into Unity main thread
        if (newDataAvailable)
        {
            string value;

            lock (lockObject)
            {
                value = threadValue;
                newDataAvailable = false;
            }

            latestRawValue = value;
            OnRawDataReceived?.Invoke(value);
        }
    }

    // =========================================================
    // SERIAL INITIALIZATION
    // =========================================================

    // Coroutine to safely open the serial port and wait for Arduino "READY"
    private IEnumerator OpenSerialPort()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            Debug.LogWarning("Serial port is already open");
            yield break;
        }
        serialPort = new SerialPort(port, bautrate)
        {
            ReadTimeout = 100, // Optional: prevents ReadLine() from sticking
            WriteTimeout = 100 // Optional: prevents Write() from sticking
        };

        try
        {
            serialPort.Open();

            // Always log that the serial port was opened
            Debug.Log("Serial port opened. Waiting for Arduino...");

        }
        catch (System.Exception e)
        {
            Debug.LogError("Error opening serial port: " + e.Message);
            yield break;
        }

        yield return new WaitForSeconds(1f);  // Wait 1 second to be sure before reading
        // Wait until Arduino sends "READY"
        while (true)
        {
            if (serialPort.IsOpen)
            {
                try
                {
                    string incoming = serialPort.ReadLine();
                    if (incoming.Trim().Equals("READY"))
                    {
                        Debug.Log("Arduino is READY");
                        break;
                    }
                }
                catch { }
            }

            yield return null;
        }
        isReady = true;
        Debug.Log("Serial port ready for communication");
        OnPortReady?.Invoke();

        // Start reading thread
        StartReadingThread();
    }

    // =========================================================
    // THREAD READING
    // =========================================================

    private void StartReadingThread()
    {
        readThread = new Thread(ReadDataThread) { IsBackground = true };
        readThread.Start();
    }
    private void ReadDataThread()
    {
        while (keepReading)
        {
            if (!IsReady || serialPort == null || !serialPort.IsOpen)
            {
                Thread.Sleep(50);
                continue;
            }

            try
            {
                string value = serialPort.ReadLine().Trim();

                lock (lockObject)
                {
                    threadValue = value;
                    newDataAvailable = true;
                }
            }
            catch
            {
                // ignore timeout/disconnect
            }

            Thread.Sleep(5);
        }
    }

    // =========================================================
    // SAFE SENDING
    // =========================================================
    public void SendSafe(string message)
    {
        if (!IsReady || serialPort == null || !serialPort.IsOpen)
        {
            Debug.LogWarning("Serial port not ready!");
            return;
        }

        if (Time.time - lastSentTime < sendCooldown)
        {
            Debug.Log("Transmit blocked (cooldown active).");
            return;
        }

        lastSentTime = Time.time;

        try
        {
            serialPort.WriteLine(message);
            Debug.Log("Sent: " + message);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Error writing to serial port: " + e.Message);
        }
    }


    // =========================================================
    // CLEAN SHUTDOWN
    // =========================================================

    void OnApplicationQuit()
    {
        keepReading = false;

        if (readThread != null && readThread.IsAlive)
            readThread.Join();

        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                if (!string.IsNullOrEmpty(messageToSerialOnStop))
                {
                    serialPort.WriteLine(messageToSerialOnStop);
                }

                serialPort.Close();
                Debug.Log("Serial port closed.");
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error closing serial port: " + e.Message);
            }
        }
    }
}
