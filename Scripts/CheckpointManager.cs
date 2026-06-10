using UnityEngine;
using System.Collections.Generic;

public class CheckpointManager : MonoBehaviour
{
    public List<Checkpoint> checkpoints = new List<Checkpoint>();
    private Dictionary<CarController, int> carCheckpoints = new Dictionary<CarController, int>();

    public void CarHitCheckpoint(CarController car, int index)
    {
        if (!carCheckpoints.ContainsKey(car))
            carCheckpoints[car] = 0;

        int expected = carCheckpoints[car];

        if (index == expected)
        {
            carCheckpoints[car]++;
            car.NextCheckpoint();

            // Last checkpoint = finish line
            if (index == checkpoints.Count - 1)
            {
                carCheckpoints[car] = 0;
                car.CompleteLap();
            }
        }
    }
}
