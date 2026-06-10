using UnityEngine;
using System.Collections.Generic;

public class AICarSimple : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    [Header("Wheel Transforms")]
    public Transform frontLeftTransform;
    public Transform frontRightTransform;
    public Transform rearLeftTransform;
    public Transform rearRightTransform;

    [Header("AI Settings")]
    public List<Transform> waypoints = new List<Transform>();
    public float motorForce = 1000f;
    public float maxSpeed = 70f;
    public float steerSensitivity = 0.5f;
    public float waypointReachDistance = 10f;

    [Header("Difficulty")]
    [Range(0.5f, 1.0f)]
    public float skillLevel = 0.85f;

    // Race position
    public int currentLap = 0;
    public int currentWaypoint = 0;
    public float distanceToNextWaypoint = 0f;

    private Rigidbody rb;
    private float currentSpeed;
    private int totalWaypoints;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
        totalWaypoints = waypoints.Count;

        // Random skill variation
        skillLevel = Random.Range(0.75f, 0.95f);
        maxSpeed = Random.Range(60f, 80f);
    }

    void FixedUpdate()
    {
        if (waypoints.Count == 0) return;
        currentSpeed = rb.linearVelocity.magnitude * 3.6f;
        Navigate();
        UpdateWheels();
    }

    void Navigate()
    {
        Transform target = waypoints[currentWaypoint];
        Vector3 localTarget = transform.InverseTransformPoint(target.position);

        // Steering
        float steer = (localTarget.x / localTarget.magnitude) * 35f * steerSensitivity;
        steer = Mathf.Clamp(steer, -35f, 35f);
        frontLeftWheel.steerAngle  = Mathf.Lerp(frontLeftWheel.steerAngle,  steer, 0.5f);
        frontRightWheel.steerAngle = Mathf.Lerp(frontRightWheel.steerAngle, steer, 0.5f);

        // Throttle
        float angle = Vector3.Angle(transform.forward,
            (target.position - transform.position).normalized);
        float throttle = skillLevel;
        if (angle > 30f) throttle *= 0.6f;
        if (angle > 60f) throttle *= 0.3f;

        // Speed control
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
        float brake = (angle > 45f && currentSpeed > 30f) ? 3000f : 0f;
        frontLeftWheel.brakeTorque  = brake;
        frontRightWheel.brakeTorque = brake;
        rearLeftWheel.brakeTorque   = brake;
        rearRightWheel.brakeTorque  = brake;

        // Check waypoint reached
        distanceToNextWaypoint = Vector3.Distance(transform.position, target.position);
        if (distanceToNextWaypoint < waypointReachDistance)
        {
            currentWaypoint++;
            if (currentWaypoint >= totalWaypoints)
            {
                currentWaypoint = 0;
                currentLap++;
            }
        }
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

    public float GetRaceProgress()
    {
        return currentLap * totalWaypoints + currentWaypoint + 
               (1f - distanceToNextWaypoint / waypointReachDistance);
    }
}
