using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCar : MonoBehaviour

{
    public GameObject player;
    public Vector3 offset;
    public float smoothTime = 0.2f;  // Lag time, the smaller the faster
    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (player == null) return;

        // Rotate offset with player
        Quaternion targetRotation = Quaternion.Euler(0, player.transform.eulerAngles.y, 0);
        Vector3 targetPosition = player.transform.position + targetRotation * offset;

        // Smooth following
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // look always at player
        transform.LookAt(player.transform.position + Vector3.up * 1.5f);
    }
}
