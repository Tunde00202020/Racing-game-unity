using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointIndex;

    void OnTriggerEnter(Collider other)
    {
        CarController car = other.GetComponent<CarController>();
        if (car == null) return;

        CheckpointManager manager = FindFirstObjectByType<CheckpointManager>();
        if (manager != null)
            manager.CarHitCheckpoint(car, checkpointIndex);
    }
}
