using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        // player
    public Vector3 offset;          // camera offset
    public float smoothSpeed = 5f;  // higher = faster follow

    void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;

        // Smooth movement
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.position = smoothedPosition;
    }
}
