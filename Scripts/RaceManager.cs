using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class RaceManager : MonoBehaviour
{
    [Header("Race Settings")]
    public int totalLaps = 3;
    public List<AICarSimple> aiCars = new List<AICarSimple>();
    public CarController playerCar;

    [Header("UI")]
    public TextMeshProUGUI positionText;
    public TextMeshProUGUI raceStatusText;

    [Header("Waypoints")]
    public List<Transform> waypoints = new List<Transform>();

    private bool raceStarted = false;
    private bool raceFinished = false;
    private float raceStartTime;
    private int playerPosition = 1;

    void Start()
    {
        // Assign waypoints to all AI cars
        foreach (var ai in aiCars)
            ai.waypoints = waypoints;

        raceStarted = true;
        raceStartTime = Time.time;

        if (raceStatusText != null)
            raceStatusText.text = "GO!";

        Invoke("ClearStatus", 2f);
    }

    void ClearStatus()
    {
        if (raceStatusText != null)
            raceStatusText.text = "";
    }

    void Update()
    {
        if (!raceStarted || raceFinished) return;
        UpdatePositions();
    }

    void UpdatePositions()
    {
        // Calculate player progress
        float playerProgress = GetPlayerProgress();

        // Count how many AI cars are ahead
        int position = 1;
        foreach (var ai in aiCars)
        {
            if (ai.GetRaceProgress() > playerProgress)
                position++;
        }

        playerPosition = position;

        if (positionText != null)
        {
            string suffix = GetSuffix(playerPosition);
            positionText.text = "POS: " + playerPosition + suffix;
        }

        // Check if player finished
        if (playerCar != null && playerCar.GetLap() >= totalLaps)
        {
            raceFinished = true;
            float raceTime = Time.time - raceStartTime;
            if (raceStatusText != null)
                raceStatusText.text = "FINISHED! " + playerPosition + 
                    GetSuffix(playerPosition) + " Place!\n" + 
                    raceTime.ToString("F2") + "s";
        }
    }

    float GetPlayerProgress()
    {
        if (playerCar == null || waypoints.Count == 0) return 0f;

        int playerLap = playerCar.GetLap();
        int nextWP = playerCar.checkpointIndex % waypoints.Count;

        float distToNext = Vector3.Distance(
            playerCar.transform.position,
            waypoints[nextWP].position);

        return playerLap * waypoints.Count + nextWP +
               Mathf.Clamp01(1f - distToNext / 20f);
    }

    string GetSuffix(int pos)
    {
        switch (pos)
        {
            case 1: return "ST";
            case 2: return "ND";
            case 3: return "RD";
            default: return "TH";
        }
    }
}
