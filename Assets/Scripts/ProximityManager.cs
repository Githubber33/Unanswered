
using UnityEngine;
using UnityEngine.Events;

public class ProximityManager : MonoBehaviour
{
    [Header("Arduino Distance Sensor")]
    public ReadSerial distanceSensor;

    [Header("Distance Settings (cm)")]
    public float minDistance = 30f;
    public float maxDistance = 250f;

    [Header("Smoothing")]
    public float smoothSpeed = 3f;

    [Header("Testing")]
    public bool testMode = false;

    [Range(0f, 500f)]
    public float testDistance = 250f;

    [Header("Current Values (Read Only)")]
    public float distanceCm;
    
    [Range(0f, 1f)]
    public float proximity;

    public ProximityStage currentStage;

    [Header("Events")]
    public UnityEvent onVisitorEnter;
    public UnityEvent onVisitorExit;

    public UnityEvent onStageChanged;

    public enum ProximityStage
    {
        Idle,
        Awareness,
        Tension,
        Anxiety,
        Confrontation
    }

    private bool visitorPresent = false;

    void Update()
    {
        // Get distance from Arduino or test mode
        if (testMode)
        {
            distanceCm = testDistance;
        }
        else
        {
            if (distanceSensor == null ||
                distanceSensor.distanceCm < 0)
                return;

            distanceCm = distanceSensor.distanceCm;
        }

        // Convert distance to proximity (0 - 1)
        float targetProximity = Mathf.InverseLerp(
            maxDistance,
            minDistance,
            distanceCm
        );

        // Smooth transitions
        proximity = Mathf.Lerp(
            proximity,
            targetProximity,
            Time.deltaTime * smoothSpeed
        );

        // Check if visitor is within range
        bool detected = distanceCm <= maxDistance;

        if (detected && !visitorPresent)
        {
            visitorPresent = true;
            Debug.Log("Visitor entered");
            onVisitorEnter?.Invoke();
        }
        else if (!detected && visitorPresent)
        {
            visitorPresent = false;
            Debug.Log("Visitor exited");
            onVisitorExit?.Invoke();
        }

        // Determine stage
        ProximityStage newStage = GetStage(proximity);

        if (newStage != currentStage)
        {
            currentStage = newStage;

            Debug.Log("Stage changed to: " + currentStage);

            onStageChanged?.Invoke();
        }
    }

    ProximityStage GetStage(float value)
    {
        if (value < 0.01f)
            return ProximityStage.Idle;

        if (value < 0.25f)
            return ProximityStage.Awareness;

        if (value < 0.5f)
            return ProximityStage.Tension;

        if (value < 0.75f)
            return ProximityStage.Anxiety;

        return ProximityStage.Confrontation;
    }
}
