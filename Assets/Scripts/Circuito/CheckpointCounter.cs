using UnityEngine;

public class CheckpointCounter : MonoBehaviour
{
    [SerializeField] private int checkpointCount = 0; // Contador interno
    [SerializeField] private GameObject stopper;

    /// <summary>
    /// Aumenta el contador de checkpoints en 1.
    /// </summary>
    public void AddCheckpoint()
    {
        checkpointCount++;
        Debug.Log($"Checkpoint alcanzado. Total: {checkpointCount}");

        if(checkpointCount >= 3)
        {
            if (stopper != null)
                stopper.SetActive(false);
        }
    }

    /// <summary>
    /// Reinicia el contador de checkpoints a 0.
    /// </summary>
    public void ResetCheckpointCount()
    {
        checkpointCount = 0;
    }

    /// <summary>
    /// Devuelve el número actual de checkpoints alcanzados.
    /// </summary>
    public int GetCheckpointCount()
    {
        return checkpointCount;
    }
}
