using UnityEngine;
using TMPro;

public class CheckpointManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [Tooltip("Number of players (columns) in the UI grid)")]
    [SerializeField] private int playerCount = 4;
    [Tooltip("Number of checkpoints (rows) in the UI grid)")]
    [SerializeField] private int checkpointCount = 3;

    [Header("UI Texts")]
    [Tooltip("Flattened array of Text fields: order should be [checkpoint0_player0, checkpoint0_player1, ..., checkpoint1_player0, ...]")]
    [SerializeField] private TextMeshProUGUI[] gridTexts;

    private void Awake()
    {
        int expected = playerCount * checkpointCount;
        if (gridTexts == null || gridTexts.Length != expected)
        {
            Debug.LogWarning($"CheckpointManager: expected {expected} UI texts (players*checkpoints), but got {(gridTexts==null?0:gridTexts.Length)}.");
        }
    }

    /// <summary>
    /// Sets the time string for a specific player and checkpoint.
    /// </summary>
    public void SetCheckpointTime(int playerIndex, int checkpointIndex, string formattedTime)
    {
        if (playerIndex < 0 || playerIndex >= playerCount) return;
        if (checkpointIndex < 0 || checkpointIndex >= checkpointCount) return;
        int idx = checkpointIndex * playerCount + playerIndex;
        if (gridTexts == null) return;
        if (idx < 0 || idx >= gridTexts.Length) return;
        if (gridTexts[idx] == null) return;
        gridTexts[idx].text = formattedTime;
    }

    /// <summary>
    /// Optional helper to format a float time (seconds) into mm:ss:ff
    /// </summary>
    public static string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int hundredths = Mathf.FloorToInt((time * 100) % 100);
        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, hundredths);
    }
}
