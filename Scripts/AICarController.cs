using UnityEngine;
using System.Collections.Generic;

public class AICarController : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    [Header("Wheel Meshes")]
    public Transform frontLeftTransform;
    public Transform frontRightTransform;
    public Transform rearLeftTransform;
    public Transform rearRightTransform;

    [Header("AI Settings")]
    public List<Transform> waypoints = new List<Transform>();
    public float motorForce    = 2500f;
    public float brakeForce    = 6000f;
    public float maxSteerAngle = 35f;
    public float maxSpeed      = 100f;
    public float waypointReachDistance = 8f;

    [Header("Difficulty")]
    [Range(0.5f, 1.0f)]
    public float skillLevel = 0.85f;

    private int   currentWaypoint = 0;
    private float currentSpeed;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
    }

    void FixedUpdate()
    {
        if (waypoints.Count == 0) return;
        Navigate();
        UpdateWheels();
    }

    void Navigate()
    {
        currentSpeed = rb.linearVelocity.magnitude * 3.6f;

        Transform target = waypoints[currentWaypoint];
        Vector3 localTarget = transform.InverseTransformPoint(target.position);

        // Steering
        float steer = (localTarget.x / localTarget.magnitude) * maxSteerAngle;
        steer *= skillLevel;
        frontLeftWheel.steerAngle  = steer;
        frontRightWheel.steerAngle = steer;

        // Throttle — slow down on sharp corners
        float angle    = Vector3.Angle(transform.forward, (target.position - transform.position).normalized);
        float throttle = (angle > 30f) ? 0.4f : skillLevel;

        // Speed cap
        if (currentSpeed < maxSpeed * skillLevel)
        {
            rearLeftWheel.motorTorque  = motorForce * throttle;
            rearRightWheel.motorTorque = motorForce * throttle;
        }
        else
        {
            rearLeftWheel.motorTorque  = 0;
            rearRightWheel.motorTorque = 0;
        }

        // Brake on sharp corners
        float brake = (angle > 45f && currentSpeed > 40f) ? brakeForce * 0.5f : 0f;
        frontLeftWheel.brakeTorque  = brake;
        frontRightWheel.brakeTorque = brake;
        rearLeftWheel.brakeTorque   = brake;
        rearRightWheel.brakeTorque  = brake;

        // Advance waypoint
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist < waypointReachDistance)
            currentWaypoint = (currentWaypoint + 1) % waypoints.Count;
    }

    void UpdateWheels()
    {
        UpdateWheelPose(frontLeftWheel,  frontLeftTransform);
        UpdateWheelPose(frontRightWheel, frontRightTransform);
        UpdateWheelPose(rearLeftWheel,   rearLeftTransform);
        UpdateWheelPose(rearRightWheel,  rearRightTransform);
    }

    void UpdateWheelPose(WheelCollider col, Transform trans)
    {
        if (trans == null) return;
        Vector3 pos; Quaternion rot;
        col.GetWorldPose(out pos, out rot);
        trans.position = pos;
        trans.rotation = rot;
    }
}
