
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class TableInteractionLighting : MonoBehaviour
{
    [Header("Installation")]
    public Transform generatedRoot;

    [Header("Seat States")]
    public bool seat1Active;
    public bool seat2Active;
    public bool seat3Active;
    public bool seat4Active;

    [Header("Hanging Lamps")]
    public float lampHeight = 2.3f;
    public float ceilingHeight = 3f;
    public float lampRadius = 0.20f;
    public float lampSpotAngle = 52f;
    public float lampOffIntensity = 0f;
    public float lampOnIntensity = 5f;

    public Color lampColor =
        new Color(1f, 0.84f, 0.62f);

    [Header("Table LED Strips")]
    public float tableWidth = 2f;
    public float tableDepth = 1.125f;
    public float ledOnIntensity = 3f;

    public Color ledColor =
        new Color(0.1f, 0.7f, 1f);

    [Header("Undertable Glow")]
    public float centerGlowMaximum = 4f;
    public float seatGlowIntensity = 2f;

    [Header("Animation")]
    public float transitionSpeed = 4f;
    public bool enablePulse = true;
    public float pulseSpeed = 1.5f;
    public float pulseStrength = 0.12f;

    private readonly Transform[] chairs = new Transform[4];
    private readonly Light[] spotlights = new Light[4];
    private readonly Light[] seatGlows = new Light[4];

    private readonly Renderer[] bulbs = new Renderer[4];
    private readonly Renderer[] ledStrips = new Renderer[4];

    private readonly Material[] bulbMaterials = new Material[4];
    private readonly Material[] ledMaterials = new Material[4];

    private Light centerGlow;
    private float tableTopHeight = 0.8f;

    private const string LightingRootName = "ReactiveLighting";

    void Start()
    {
        if (generatedRoot == null)
            generatedRoot = transform.Find("GeneratedInstallation");

        if (generatedRoot == null)
        {
            Debug.LogError(
                "GeneratedInstallation not found!"
            );
            enabled = false;
            return;
        }

        FindChairs();
        FindTableHeight();
        BuildLighting();
    }

    void Update()
    {
        HandleKeyboard();
        UpdateLights();
    }

    // =====================================
    // FIND EXISTING OBJECTS
    // =====================================

    void FindChairs()
    {
        Transform chairsRoot =
            generatedRoot.Find("Four Chairs");

        if (chairsRoot == null)
        {
            Debug.LogError("Four Chairs folder not found!");
            return;
        }

        for (int i = 0; i < 4; i++)
        {
            chairs[i] =
                chairsRoot.Find("Chair " + (i + 1));

            if (chairs[i] == null)
                Debug.LogWarning(
                    "Chair " + (i + 1) + " not found!"
                );
        }
    }

    void FindTableHeight()
    {
        Transform screen = generatedRoot.Find(
            "Interactive Table/Video Projection Surface"
        );

        if (screen != null)
            tableTopHeight = screen.position.y;
    }

    // =====================================
    // BUILD LIGHTING INSTALLATION
    // =====================================

    void BuildLighting()
    {
        Transform existing =
            generatedRoot.Find(LightingRootName);

        if (existing != null)
        {
            if (Application.isPlaying)
                Destroy(existing.gameObject);
            else
                DestroyImmediate(existing.gameObject);
        }

        Transform root =
            new GameObject(LightingRootName).transform;

        root.SetParent(generatedRoot, false);

        Material metal = CreateMaterial(
            new Color(0.07f, 0.08f, 0.09f)
        );

        Material shade = CreateMaterial(
            new Color(0.18f, 0.19f, 0.21f)
        );

        for (int i = 0; i < 4; i++)
        {
            if (chairs[i] == null)
                continue;

            Vector3 chairPosition =
                chairs[i].position;

            CreateHangingLamp(
                root,
                i,
                chairPosition,
                metal,
                shade
            );

            CreateSeatGlow(root, i, chairPosition);

            CreateLEDStrip(root, i);
        }

        CreateCenterGlow(root);
    }

    // =====================================
    // HANGING SPOTLIGHTS
    // =====================================

    void CreateHangingLamp(
        Transform parent,
        int index,
        Vector3 chairPosition,
        Material metal,
        Material shade)
    {
        Transform lamp =
            new GameObject(
                "Hanging Lamp " + (index + 1)
            ).transform;

        lamp.SetParent(parent, false);

        Vector3 position = new Vector3(
            chairPosition.x,
            lampHeight,
            chairPosition.z
        );

        lamp.position = position;

        // Ceiling cable
        float cableLength =
            Mathf.Max(0.1f, ceilingHeight - lampHeight);

        CreateCylinder(
            "Hanging Cable",
            lamp,
            new Vector3(
                0,
                cableLength / 2f + 0.10f,
                0
            ),
            new Vector3(
                0.012f,
                cableLength / 2f,
                0.012f
            ),
            metal
        );

        // Ceiling mount
        CreateCylinder(
            "Ceiling Mount",
            lamp,
            new Vector3(
                0,
                cableLength + 0.10f,
                0
            ),
            new Vector3(0.10f, 0.025f, 0.10f),
            metal
        );

        // Lamp housing
        CreateCylinder(
            "Lamp Housing",
            lamp,
            Vector3.zero,
            new Vector3(
                lampRadius,
                0.10f,
                lampRadius
            ),
            shade
        );

        // Lamp bulb
        GameObject bulb =
            GameObject.CreatePrimitive(
                PrimitiveType.Sphere
            );

        bulb.name = "Light Bulb";
        bulb.transform.SetParent(lamp, false);

        bulb.transform.localPosition =
            new Vector3(0, -0.13f, 0);

        bulb.transform.localScale =
            Vector3.one * 0.10f;

        Material bulbMat =
            CreateMaterial(Color.black);

        bulbMaterials[index] = bulbMat;

        bulbs[index] =
            bulb.GetComponent<Renderer>();

        bulbs[index].sharedMaterial = bulbMat;

        // Actual Unity spotlight
        GameObject lightObj =
            new GameObject("Personal Spotlight");

        lightObj.transform.SetParent(lamp, false);

        lightObj.transform.localPosition =
            new Vector3(0, -0.22f, 0);

        // Aim at the seated person's upper body
        Vector3 target = new Vector3(
            chairPosition.x,
            0.95f,
            chairPosition.z
        );

        lightObj.transform.LookAt(target);

        Light light =
            lightObj.AddComponent<Light>();

        light.type = LightType.Spot;
        light.spotAngle = lampSpotAngle;
        light.range = 4f;
        light.intensity = lampOffIntensity;
        light.color = lampColor;
        light.shadows = LightShadows.Soft;

        spotlights[index] = light;
    }

    // =====================================
    // PERSONAL TABLE GLOW
    // =====================================

    void CreateSeatGlow(
        Transform parent,
        int index,
        Vector3 chairPosition)
    {
        GameObject obj =
            new GameObject(
                "Seat Glow " + (index + 1)
            );

        obj.transform.SetParent(parent, false);

        float side =
            chairPosition.z < 0 ? -1f : 1f;

        obj.transform.position = new Vector3(
            chairPosition.x,
            tableTopHeight - 0.18f,
            side * (tableDepth / 2f)
        );

        Light light = obj.AddComponent<Light>();

        light.type = LightType.Point;
        light.range = 1.6f;
        light.color = ledColor;
        light.intensity = 0f;
        light.shadows = LightShadows.None;

        seatGlows[index] = light;
    }

    // =====================================
    // TABLE EDGE LED STRIPS
    // =====================================

    void CreateLEDStrip(
        Transform parent,
        int index)
    {
        if (chairs[index] == null)
            return;

        Vector3 chairPosition =
            chairs[index].position;

        float side =
            chairPosition.z < 0 ? -1f : 1f;

        GameObject strip =
            GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );

        strip.name =
            "LED Strip " + (index + 1);

        strip.transform.SetParent(parent, false);

        strip.transform.position = new Vector3(
            chairPosition.x,
            tableTopHeight - 0.055f,
            side * (tableDepth / 2f + 0.015f)
        );

        strip.transform.localScale =
            new Vector3(
                tableWidth * 0.44f,
                0.015f,
                0.025f
            );

        Material material =
            CreateMaterial(Color.black);

        ledMaterials[index] = material;

        Renderer renderer =
            strip.GetComponent<Renderer>();

        renderer.sharedMaterial = material;

        ledStrips[index] = renderer;
    }

    // =====================================
    // CENTRAL UNDER-TABLE LIGHT
    // =====================================

    void CreateCenterGlow(Transform parent)
    {
        GameObject obj =
            new GameObject("Collective Glow");

        obj.transform.SetParent(parent, false);

        obj.transform.position = new Vector3(
            generatedRoot.position.x,
            tableTopHeight - 0.35f,
            generatedRoot.position.z
        );

        centerGlow = obj.AddComponent<Light>();

        centerGlow.type = LightType.Point;
        centerGlow.range = 3f;
        centerGlow.color = ledColor;
        centerGlow.intensity = 0f;
        centerGlow.shadows = LightShadows.None;
    }

    // =====================================
    // KEYBOARD CONTROLS
    // =====================================

    void HandleKeyboard()
    {
#if ENABLE_INPUT_SYSTEM

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.digit1Key.wasPressedThisFrame)
            seat1Active = !seat1Active;

        if (keyboard.digit2Key.wasPressedThisFrame)
            seat2Active = !seat2Active;

        if (keyboard.digit3Key.wasPressedThisFrame)
            seat3Active = !seat3Active;

        if (keyboard.digit4Key.wasPressedThisFrame)
            seat4Active = !seat4Active;

        if (keyboard.aKey.wasPressedThisFrame)
            SetSeatStates(true, true, true, true);

        if (keyboard.spaceKey.wasPressedThisFrame)
            SetSeatStates(false, false, false, false);

#elif ENABLE_LEGACY_INPUT_MANAGER

        if (Input.GetKeyDown(KeyCode.Alpha1))
            seat1Active = !seat1Active;

        if (Input.GetKeyDown(KeyCode.Alpha2))
            seat2Active = !seat2Active;

        if (Input.GetKeyDown(KeyCode.Alpha3))
            seat3Active = !seat3Active;

        if (Input.GetKeyDown(KeyCode.Alpha4))
            seat4Active = !seat4Active;

        if (Input.GetKeyDown(KeyCode.A))
            SetSeatStates(true, true, true, true);

        if (Input.GetKeyDown(KeyCode.Space))
            SetSeatStates(false, false, false, false);

#endif
    }

    // =====================================
    // UPDATE ALL LIGHTS
    // =====================================

    void UpdateLights()
    {
        bool[] states =
        {
            seat1Active,
            seat2Active,
            seat3Active,
            seat4Active
        };

        int activeCount = 0;

        foreach (bool active in states)
        {
            if (active)
                activeCount++;
        }

        float smooth =
            1f - Mathf.Exp(
                -transitionSpeed * Time.deltaTime
            );

        float pulse = 1f;

        if (enablePulse)
        {
            pulse = 1f +
                Mathf.Sin(
                    Time.time * pulseSpeed
                ) * pulseStrength;
        }

        for (int i = 0; i < 4; i++)
        {
            bool active = states[i];

            // Overhead spotlights
            if (spotlights[i] != null)
            {
                float intensity = active
                    ? lampOnIntensity * pulse
                    : lampOffIntensity;

                spotlights[i].intensity = Mathf.Lerp(
                    spotlights[i].intensity,
                    intensity,
                    smooth
                );
            }

            // Light underneath each seat zone
            if (seatGlows[i] != null)
            {
                float intensity = active
                    ? seatGlowIntensity * pulse
                    : 0f;

                seatGlows[i].intensity = Mathf.Lerp(
                    seatGlows[i].intensity,
                    intensity,
                    smooth
                );
            }

            // Physical LED-style strip
            if (ledMaterials[i] != null)
            {
                Color target = active
                    ? ledColor * ledOnIntensity * pulse
                    : Color.black;

                UpdateEmission(
                    ledMaterials[i],
                    target,
                    smooth
                );
            }

            // Visible bulbs
            if (bulbMaterials[i] != null)
            {
                Color target = active
                    ? lampColor * 3f * pulse
                    : Color.black;

                UpdateEmission(
                    bulbMaterials[i],
                    target,
                    smooth
                );
            }
        }

        // More people = stronger collective light
        if (centerGlow != null)
        {
            float participation =
                activeCount / 4f;

            float target =
                centerGlowMaximum *
                participation *
                pulse;

            centerGlow.intensity = Mathf.Lerp(
                centerGlow.intensity,
                target,
                smooth
            );
        }
    }

    // =====================================
    // MATERIALS
    // =====================================

    Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find(
            "Universal Render Pipeline/Lit"
        );

        if (shader == null)
            shader = Shader.Find("Standard");

        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        Material mat = new Material(shader);

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);

        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", color);

        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor(
                "_EmissionColor",
                Color.black
            );
        }

        return mat;
    }

    void UpdateEmission(
        Material material,
        Color target,
        float smooth)
    {
        if (material.HasProperty("_EmissionColor"))
        {
            Color current =
                material.GetColor("_EmissionColor");

            material.SetColor(
                "_EmissionColor",
                Color.Lerp(current, target, smooth)
            );
        }

        // Also make the mesh itself visibly change color
        Color baseColor =
            Color.Lerp(
                Color.black,
                target,
                0.25f
            );

        if (material.HasProperty("_BaseColor"))
            material.SetColor(
                "_BaseColor",
                baseColor
            );

        if (material.HasProperty("_Color"))
            material.SetColor(
                "_Color",
                baseColor
            );
    }

    GameObject CreateCylinder(
        string name,
        Transform parent,
        Vector3 position,
        Vector3 scale,
        Material material)
    {
        GameObject obj =
            GameObject.CreatePrimitive(
                PrimitiveType.Cylinder
            );

        obj.name = name;

        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = position;
        obj.transform.localScale = scale;

        obj.GetComponent<Renderer>().sharedMaterial =
            material;

        return obj;
    }

    // =====================================
    // ARDUINO INTEGRATION
    // =====================================

    // Seat index is 0-3.
    public void SetSeatState(int seatIndex, bool active)
    {
        switch (seatIndex)
        {
            case 0:
                seat1Active = active;
                break;

            case 1:
                seat2Active = active;
                break;

            case 2:
                seat3Active = active;
                break;

            case 3:
                seat4Active = active;
                break;
        }
    }

    public void SetSeatStates(
        bool s1,
        bool s2,
        bool s3,
        bool s4)
    {
        seat1Active = s1;
        seat2Active = s2;
        seat3Active = s3;
        seat4Active = s4;
    }
}
