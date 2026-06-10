using UnityEngine;
using TMPro;

public class CarController : MonoBehaviour
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

    [Header("Car Settings")]
    public float motorForce = 3000f;
    public float brakeForce = 8000f;
    public float maxSteerAngle = 30f;
    public float maxSpeed = 120f;

    [Header("Nitro")]
    public float nitroForce = 5000f;
    public float nitroMax = 100f;
    public float nitroCurrent = 100f;
    public float nitroDrainRate = 20f;
    public float nitroRechargeRate = 8f;
    public bool nitroActive = false;

    [Header("Camera")]
    public Camera carCamera;
    public float normalFOV = 60f;
    public float nitroFOV = 80f;
    public float maxSpeedFOV = 75f;

    [Header("UI")]
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI nitroText;
    public TextMeshProUGUI lapText;
    public TextMeshProUGUI positionText;
    public TextMeshProUGUI lapTimeText;

    [Header("Effects")]
    public ParticleSystem[] smokeParticles;
    public ParticleSystem nitroEffect;
    public TrailRenderer[] skidmarks;

    // Private
    private float horizontalInput;
    private float verticalInput;
    private bool isBraking;
    private Rigidbody rb;
    private float currentSpeed;
    private bool isSkidding;

    // Lap system
    public int currentLap = 0;
    private int totalLaps = 3;
    private float lapStartTime;
    private float bestLapTime = float.MaxValue;
    public int checkpointIndex = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
        lapStartTime = Time.time;
    }

    void Update()
    {
        GetInput();
        UpdateUI();
        UpdateCamera();
        UpdateEffects();
    }

    void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    void GetInput()
    {
        horizontalInput = 0f;
        verticalInput   = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  horizontalInput = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontalInput =  1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))    verticalInput   =  1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))  verticalInput   = -1f;

        isBraking  = Input.GetKey(KeyCode.Space);
        nitroActive = Input.GetKey(KeyCode.LeftShift) && nitroCurrent > 0;
    }

    void HandleMotor()
    {
        currentSpeed = rb.linearVelocity.magnitude * 3.6f; // km/h

        float force = motorForce * verticalInput;

        // Nitro boost
        if (nitroActive)
        {
            force += nitroForce;
            nitroCurrent -= nitroDrainRate * Time.fixedDeltaTime;
            nitroCurrent = Mathf.Clamp(nitroCurrent, 0, nitroMax);
        }
        else
        {
            nitroCurrent += nitroRechargeRate * Time.fixedDeltaTime;
            nitroCurrent = Mathf.Clamp(nitroCurrent, 0, nitroMax);
        }

        // Speed cap
        if (currentSpeed < maxSpeed || verticalInput < 0)
        {
            rearLeftWheel.motorTorque  = force;
            rearRightWheel.motorTorque = force;
        }
        else
        {
            rearLeftWheel.motorTorque  = 0;
            rearRightWheel.motorTorque = 0;
        }

        // Braking
        float brake = isBraking ? brakeForce : 0f;
        frontLeftWheel.brakeTorque  = brake;
        frontRightWheel.brakeTorque = brake;
        rearLeftWheel.brakeTorque   = brake;
        rearRightWheel.brakeTorque  = brake;
    }

    void HandleSteering()
    {
        float steerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheel.steerAngle  = steerAngle;
        frontRightWheel.steerAngle = steerAngle;
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

    void UpdateCamera()
    {
        if (carCamera == null) return;
        float targetFOV = normalFOV;
        if (nitroActive)
            targetFOV = nitroFOV;
        else
            targetFOV = Mathf.Lerp(normalFOV, maxSpeedFOV, currentSpeed / maxSpeed);
        carCamera.fieldOfView = Mathf.Lerp(carCamera.fieldOfView, targetFOV, Time.deltaTime * 3f);
    }

    void UpdateUI()
    {
        if (speedText != null)
            speedText.text = Mathf.RoundToInt(currentSpeed) + " km/h";
        if (nitroText != null)
            nitroText.text = "NITRO: " + Mathf.RoundToInt(nitroCurrent) + "%";
        if (lapText != null)
            lapText.text = "LAP: " + currentLap + " / " + totalLaps;
        float lapTime = Time.time - lapStartTime;
        if (lapTimeText != null)
            lapTimeText.text = "TIME: " + lapTime.ToString("F2") + "s";
    }

    void UpdateEffects()
    {
        // Skid marks when braking or drifting
        isSkidding = isBraking && currentSpeed > 10f;
        if (skidmarks != null)
            foreach (var sk in skidmarks)
                if (sk != null) sk.emitting = isSkidding;

        // Smoke when skidding
        if (smokeParticles != null)
            foreach (var smoke in smokeParticles)
                if (smoke != null)
                {
                    var em = smoke.emission;
                    em.enabled = isSkidding;
                }

        // Nitro effect
        if (nitroEffect != null)
        {
            if (nitroActive && !nitroEffect.isPlaying)
                nitroEffect.Play();
            else if (!nitroActive && nitroEffect.isPlaying)
                nitroEffect.Stop();
        }
    }

    // Called by CheckpointManager
    public void CompleteLap()
    {
        currentLap++;
        float lapTime = Time.time - lapStartTime;
        if (lapTime < bestLapTime) bestLapTime = lapTime;
        lapStartTime = Time.time;
        if (currentLap >= totalLaps)
            Debug.Log("RACE FINISHED! Best lap: " + bestLapTime.ToString("F2") + "s");
    }

    public void NextCheckpoint()
    {
        checkpointIndex++;
    }

    public int GetLap() => currentLap;
    public float GetSpeed() => currentSpeed;
    public float GetNitro() => nitroCurrent;
}
