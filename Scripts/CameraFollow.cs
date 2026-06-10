using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float distance    = 6f;
    public float height      = 2.5f;
    public float smoothSpeed = 8f;
    public float lookAheadDistance = 3f;

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // Position behind and above car
        Vector3 desiredPos = target.position
            - target.forward * distance
            + Vector3.up * height;

        transform.position = Vector3.SmoothDamp(
            transform.position, desiredPos, ref velocity, 1f / smoothSpeed);

        // Look slightly ahead of car
        Vector3 lookTarget = target.position + target.forward * lookAheadDistance;
        transform.LookAt(lookTarget);
    }
}
