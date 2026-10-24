using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Builds a real-scale gallery installation for "The Table Between Us".
/// Drag a table and chair prefab into the component, then choose
/// "Generate Installation" from the component's context menu.
/// Video projection is simulated by a VideoPlayer/RenderTexture on the tabletop.
/// </summary>
public class TableInstallationBuilder : MonoBehaviour
{
    [Header("Furniture prefabs (from Furniture Mega Pack)")]
    public GameObject tablePrefab;
    public GameObject chairPrefab;

    [Tooltip("Corrects a sideways/upside-down table model without modifying the original prefab.")]
    public Vector3 tableModelRotation = Vector3.zero;
    [Tooltip("Corrects a sideways/upside-down chair model without modifying the original prefab.")]
    public Vector3 chairModelRotation = Vector3.zero;
    [Tooltip("Turn all chairs around their vertical axes if their fronts face away from the table.")]
    public float chairFacingOffset = 0f;

    [Header("Furniture size, in metres")]
    [Min(0.5f)] public float tableWidth = 2f;
    [Min(0.5f)] public float tableDepth = 1.125f;
    [Min(0.3f)] public float tableHeight = 0.8f;
    [Min(0.25f)] public float chairWidth = 0.5f;
    [Min(0.25f)] public float chairDepth = 0.55f;
    [Min(0.4f)] public float chairOverallHeight = 0.95f;
    [Min(0.1f)] public float chairGapFromTable = 0.24f;

    [Header("Video on table (16:9)")]
    public VideoClip videoClip;
    public bool loopVideo = true;
    public bool playAudio = true;
    [Range(0.5f, 0.98f)] public float screenWidthFraction = 0.90f;

    [Header("Room and projector")]
    public Vector2 roomSize = new Vector2(5f, 4f);
    public float projectorHeight = 2.7f;
    public bool createTwoWalls = true;

    private const string GeneratedName = "GeneratedInstallation";
    private RenderTexture videoTexture;
    private Material videoMaterial;
    private Material unlitPlaceholder;
    private VideoPlayer videoPlayer;

    private void Start()
    {
        if (transform.Find(GeneratedName) == null)
            GenerateInstallation();

        StartVideo();
    }

    [ContextMenu("Generate Installation")]
    public void GenerateInstallation()
    {
        // Keep this generator GameObject at position 0, rotation 0, scale 1.
        Transform old = transform.Find(GeneratedName);
        if (old != null)
        {
            if (Application.isPlaying) Destroy(old.gameObject);
            else DestroyImmediate(old.gameObject);
        }

        Transform root = new GameObject(GeneratedName).transform;
        root.SetParent(transform, false);

        Material concrete = MakeLit("Concrete", new Color(0.35f, 0.36f, 0.37f), 0f, 0.28f);
        Material wall = MakeLit("Dark gallery wall", new Color(0.16f, 0.18f, 0.20f), 0f, 0.12f);
        Material black = MakeLit("Black metal", new Color(0.07f, 0.08f, 0.09f), 0.75f, 0.55f);
        Material wood = MakeLit("Walnut", new Color(0.37f, 0.22f, 0.12f), 0f, 0.35f);
        Material sensorMat = MakeLit("Sensor", new Color(0.93f, 0.55f, 0.13f), 0.25f, 0.45f);
        Material projectorMat = MakeLit("Projector shell", new Color(0.78f, 0.79f, 0.81f), 0.15f, 0.5f);
        unlitPlaceholder = MakeUnlit(new Color(0.03f, 0.06f, 0.08f));

        // ROOM: an open-front gallery for an unobstructed perspective camera.
        Transform room = Group("Room", root);
        Cube("Floor", room, new Vector3(0, -0.10f, 0),
            new Vector3(roomSize.x, 0.2f, roomSize.y), concrete);
        if (createTwoWalls)
        {
            Cube("Rear wall", room, new Vector3(0, 1.5f, roomSize.y / 2f),
                new Vector3(roomSize.x, 3f, 0.1f), wall);
            Cube("Left wall", room, new Vector3(-roomSize.x / 2f, 1.5f, 0),
                new Vector3(0.1f, 3f, roomSize.y), wall);
        }

        // TABLE: use the prefab, resized to a 200 x 112.5 cm tabletop by default.
        Transform table = Group("Interactive Table", root);
        if (tablePrefab != null)
        {
            PlacePrefab(tablePrefab, table, "Furniture Pack Table", Vector3.zero,
                tableModelRotation, new Vector3(tableWidth, tableHeight, tableDepth),
                new Vector3(0f, tableHeight / 2f, 0f));
        }
        else
        {
            Cube("Fallback tabletop", table,
                new Vector3(0, tableHeight - 0.05f, 0),
                new Vector3(tableWidth, 0.10f, tableDepth), wood);
            foreach (float x in new[] { -tableWidth / 2f + 0.12f, tableWidth / 2f - 0.12f })
            foreach (float z in new[] { -tableDepth / 2f + 0.12f, tableDepth / 2f - 0.12f })
                Cube("Fallback table leg", table,
                    new Vector3(x, (tableHeight - 0.1f) / 2f, z),
                    new Vector3(0.06f, tableHeight - 0.1f, 0.06f), black);
        }

        // The projection is always a precise 16:9 rectangle, within the tabletop.
        float screenW = Mathf.Min(tableWidth * screenWidthFraction,
            tableDepth * 16f / 9f * screenWidthFraction);
        float screenD = screenW * 9f / 16f;
        Cube("Dark projection inset", table,
            new Vector3(0f, tableHeight + 0.002f, 0f),
            new Vector3(screenW + 0.025f, 0.003f, screenD + 0.025f), black);

        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
        screen.name = "Video Projection Surface";
        screen.transform.SetParent(table, false);
        screen.transform.localPosition = new Vector3(0f, tableHeight + 0.006f, 0f);
        screen.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        screen.transform.localScale = new Vector3(screenW, screenD, 1f);
        screen.GetComponent<Renderer>().sharedMaterial = unlitPlaceholder;
        // Double-check orientation: the quad's front faces up after +90 deg about X.
        // (If you prefer the video rotated 180 degrees, change its local Y rotation.)

        // CHAIRS: exactly four, two on each long side (the +/- Z sides).
        Transform chairs = Group("Chairs", root);
        Transform sensors = Group("Arduino Sensors", root);
        float seatX = tableWidth * 0.26f;
        float seatZ = tableDepth / 2f + chairGapFromTable + chairDepth / 2f;
        int index = 1;
        foreach (float side in new[] { -1f, 1f })
        foreach (float xSide in new[] { -1f, 1f })
        {
            float x = xSide * seatX;
            float z = side * seatZ;
            Transform chairGroup = Group("Chair_" + index, chairs);
            chairGroup.localPosition = new Vector3(x, 0, z);
            chairGroup.localRotation = Quaternion.Euler(
                0, (side < 0 ? 0f : 180f) + chairFacingOffset, 0);

            if (chairPrefab != null)
            {
                // Correct prefab orientation, size and pivot relative to the group.
                PlacePrefab(chairPrefab, chairGroup, "Furniture Pack Chair",
                    Vector3.zero, chairModelRotation,
                    new Vector3(chairWidth, chairOverallHeight, chairDepth),
                    new Vector3(0, chairOverallHeight / 2f, 0));
            }
            else
            {
                Cube("Fallback seat", chairGroup,
                    new Vector3(0, 0.45f, 0),
                    new Vector3(0.48f, 0.08f, 0.48f), wood);
                Cube("Fallback backrest", chairGroup,
                    new Vector3(0, 0.73f, -0.23f),
                    new Vector3(0.48f, 0.52f, 0.06f), black);
                foreach (float lx in new[] { -0.19f, 0.19f })
                foreach (float lz in new[] { -0.19f, 0.19f })
                    Cube("Fallback chair leg", chairGroup,
                        new Vector3(lx, 0.21f, lz),
                        new Vector3(0.04f, 0.42f, 0.04f), black);
            }

            // Small sensor housing on each long table edge.
            Transform sensor = Group("Sensor_" + index, sensors);
            sensor.localPosition = new Vector3(x, tableHeight - 0.08f,
                side * (tableDepth / 2f + 0.01f));
            Cube("Housing", sensor, Vector3.zero,
                new Vector3(0.09f, 0.05f, 0.055f), black);
            foreach (float lensX in new[] { -0.022f, 0.022f })
            {
                GameObject lens = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lens.name = "Transducer";
                lens.transform.SetParent(sensor, false);
                lens.transform.localPosition = new Vector3(lensX, 0, side * 0.031f);
                lens.transform.localRotation = Quaternion.Euler(90, 0, 0);
                lens.transform.localScale = new Vector3(0.016f, 0.003f, 0.016f);
                lens.GetComponent<Renderer>().sharedMaterial = sensorMat;
            }
            index++;
        }

        // CEILING PROJECTOR (physical housing). Video is mapped on the table.
        Transform projector = Group("Projector", root);
        Cube("Body", projector, new Vector3(0, projectorHeight, 0),
            new Vector3(0.33f, 0.14f, 0.40f), projectorMat);
        Cube("Bottom panel", projector, new Vector3(0, projectorHeight - 0.075f, 0),
            new Vector3(0.30f, 0.015f, 0.37f), black);
        GameObject projectorLens = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        projectorLens.name = "Lens";
        projectorLens.transform.SetParent(projector, false);
        projectorLens.transform.localPosition = new Vector3(0, projectorHeight - 0.095f, 0);
        projectorLens.transform.localScale = new Vector3(0.06f, 0.025f, 0.06f);
        projectorLens.GetComponent<Renderer>().sharedMaterial = black;
        Cube("Mount", projector, new Vector3(0, projectorHeight + 0.23f, 0),
            new Vector3(0.045f, 0.32f, 0.045f), black);
        videoPlayer = projector.gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;

        // LIGHTING
        Transform lights = Group("Lighting", root);
        GameObject key = new GameObject("Soft gallery light");
        key.transform.SetParent(lights, false);
        key.transform.localRotation = Quaternion.Euler(48f, -32f, 0);
        Light directional = key.AddComponent<Light>();
        directional.type = LightType.Directional;
        directional.intensity = 0.7f;
        directional.shadows = LightShadows.Soft;

        GameObject fill = new GameObject("Warm fill");
        fill.transform.SetParent(lights, false);
        fill.transform.localPosition = new Vector3(-1.4f, 2.4f, -0.7f);
        Light fillLight = fill.AddComponent<Light>();
        fillLight.type = LightType.Point;
        fillLight.color = new Color(1f, 0.86f, 0.72f);
        fillLight.intensity = 2.3f;
        fillLight.range = 4.5f;

        // CAMERAS (turn off/delete the original scene Main Camera).
        Transform cameras = Group("Cameras", root);
        GameObject sketch = new GameObject("SketchCamera");
        sketch.transform.SetParent(cameras, false);
        sketch.transform.localPosition = new Vector3(3.4f, 2.7f, -3.6f);
        sketch.transform.LookAt(new Vector3(0, tableHeight, 0));
        Camera sketchCamera = sketch.AddComponent<Camera>();
        sketchCamera.fieldOfView = 60f;
        sketch.tag = "MainCamera";

        GameObject plan = new GameObject("FloorPlanCamera");
        plan.transform.SetParent(cameras, false);
        plan.transform.localPosition = new Vector3(0, 7, 0);
        plan.transform.localRotation = Quaternion.Euler(90, 0, 0);
        Camera planCamera = plan.AddComponent<Camera>();
        planCamera.orthographic = true;
        planCamera.orthographicSize = 3.1f;
        planCamera.enabled = false;

        Debug.Log("The Table Between Us: generated. Assign table/chair prefabs and a video in the Inspector.");
        if (Application.isPlaying) StartVideo();
    }

    // Fits a prefab's visible Renderer bounds to a requested size, correcting
    // non-centred pivots and awkward import scales, without changing prefab files.
    private void PlacePrefab(GameObject prefab, Transform parent, string objectName,
        Vector3 localPosition, Vector3 modelEuler, Vector3 targetSize,
        Vector3 targetBoundsCenter)
    {
        GameObject model = Instantiate(prefab, parent);
        model.name = objectName;
        model.transform.localPosition = localPosition;
        model.transform.localRotation = Quaternion.Euler(modelEuler);

        Bounds before;
        if (!TryGetBounds(model, out before))
        {
            Debug.LogWarning("No Renderer found on " + prefab.name);
            return;
        }

        // Renderer bounds are world-space axis-aligned; adjust each parent-local
        // model axis. This works best when the parent has no nonuniform scale.
        Vector3 s = model.transform.localScale;
        s.x *= targetSize.x / Mathf.Max(0.001f, before.size.x);
        s.y *= targetSize.y / Mathf.Max(0.001f, before.size.y);
        s.z *= targetSize.z / Mathf.Max(0.001f, before.size.z);
        model.transform.localScale = s;

        Bounds after;
        if (TryGetBounds(model, out after))
        {
            Vector3 worldTarget = parent.TransformPoint(targetBoundsCenter);
            model.transform.position += worldTarget - after.center;
        }
    }

    private bool TryGetBounds(GameObject obj, out Bounds bounds)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        bounds = new Bounds();
        bool found = false;
        foreach (Renderer r in renderers)
        {
            if (!found) { bounds = r.bounds; found = true; }
            else bounds.Encapsulate(r.bounds);
        }
        return found;
    }

    private void StartVideo()
    {
        if (videoClip == null)
        {
            Debug.Log("Add a video to the Video Clip field for tabletop playback.");
            return;
        }

        Transform root = transform.Find(GeneratedName);
        if (root == null) return;
        Transform screen = root.Find("Interactive Table/Video Projection Surface");
        Transform projector = root.Find("Projector");
        if (screen == null || projector == null) return;

        videoPlayer = projector.GetComponent<VideoPlayer>();
        if (videoPlayer == null)
            videoPlayer = projector.gameObject.AddComponent<VideoPlayer>();

        // Prevent leaked textures when generating repeatedly during Play Mode.
        if (videoTexture != null) { videoTexture.Release(); Destroy(videoTexture); }
        if (videoMaterial != null) Destroy(videoMaterial);

        videoTexture = new RenderTexture(1920, 1080, 0);
        videoTexture.name = "TableVideo_1920x1080";
        videoTexture.Create();
        videoMaterial = MakeUnlit(Color.white);
        videoMaterial.mainTexture = videoTexture;
        if (videoMaterial.HasProperty("_BaseMap"))
            videoMaterial.SetTexture("_BaseMap", videoTexture);
        screen.GetComponent<Renderer>().material = videoMaterial;

        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = videoClip;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = videoTexture;
        videoPlayer.isLooping = loopVideo;
        videoPlayer.playOnAwake = false;
        videoPlayer.audioOutputMode = playAudio ?
            VideoAudioOutputMode.AudioSource : VideoAudioOutputMode.None;

        if (playAudio)
        {
            AudioSource audio = projector.GetComponent<AudioSource>();
            if (audio == null) audio = projector.gameObject.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.spatialBlend = 0f;
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetTargetAudioSource(0, audio);
        }
        videoPlayer.Play();
    }

    private Material MakeLit(string name, Color color, float metallic, float smoothness)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        Material mat = new Material(shader) { name = name };
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
        return mat;
    }

    private Material MakeUnlit(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Texture");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        Material mat = new Material(shader);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        return mat;
    }

    private Transform Group(string name, Transform parent)
    {
        Transform t = new GameObject(name).transform;
        t.SetParent(parent, false);
        return t;
    }

    private GameObject Cube(string name, Transform parent,
        Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().sharedMaterial = material;
        return cube;
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.targetTexture = null;
        }
        if (videoTexture != null)
        {
            videoTexture.Release();
            Destroy(videoTexture);
        }
        if (videoMaterial != null) Destroy(videoMaterial);
    }
}
