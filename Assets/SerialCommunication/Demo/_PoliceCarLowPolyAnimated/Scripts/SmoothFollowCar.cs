using UnityEngine;

public class SmoothFollowCar : MonoBehaviour
{
    public Transform player;           // De auto
    public Vector3 offset = new Vector3(0, 5, -10); // Standaard camera offset
    public float positionSmoothTime = 0.2f; // Vloeiende positie
    public float rotationSmoothTime = 0.1f; // Vloeiende rotatie
    public float lookHeight = 1.5f;   // Hoe hoog de camera kijkt naar de auto

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (player == null) return;

        // 1️⃣ Roteer offset mee met speler
        Quaternion rotation = Quaternion.Euler(0, player.eulerAngles.y, 0);
        Vector3 targetPosition = player.position + rotation * offset;

        // 2️⃣ Smooth volgpositie
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, positionSmoothTime);

        // 3️⃣ Smooth rotatie naar speler
        Vector3 lookTarget = player.position + Vector3.up * lookHeight;
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothTime * Time.deltaTime * 60f);
    }
}