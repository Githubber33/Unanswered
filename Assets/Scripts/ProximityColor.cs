
using UnityEngine;

public class ProximityColor : MonoBehaviour
{
    [Header("Proximity Manager")]
    public ProximityManager proximityManager;

    [Header("Color Settings")]
    [Range(0f, 1f)]
    public float maxHue = 0.75f;

    public float smoothSpeed = 5f;

    private Material objectMaterial;

    void Start()
    {
        objectMaterial = GetComponent<Renderer>().material;
    }

    void Update()
    {
        if (proximityManager == null)
            return;

        float proximity = proximityManager.proximity;

        // Convert proximity to color spectrum
        float hue = proximity * maxHue;

        Color targetColor = Color.HSVToRGB(
            hue,
            1f,
            1f
        );

        // Smoothly change material color
        objectMaterial.color = Color.Lerp(
            objectMaterial.color,
            targetColor,
            Time.deltaTime * smoothSpeed
        );
    }

    void OnDestroy()
    {
        if (objectMaterial != null)
            Destroy(objectMaterial);
    }
}
