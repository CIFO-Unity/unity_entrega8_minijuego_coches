using UnityEngine;
using TMPro;

public class Checkpoint : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StopwatchTimer stopwatchTimer;   // Referencia al cronómetro
    [SerializeField] private TextMeshProUGUI checkpointTimeText; // Texto donde mostrar el tiempo

    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";     // Tag del jugador

    private bool checkpointReached = false; // Evita registrar múltiples veces el mismo checkpoint

    private void OnTriggerEnter(Collider other)
    {
        // Comprobar que sea el jugador y que aún no se haya activado
        if (!checkpointReached && other.CompareTag(playerTag))
        {
            checkpointReached = true;

            if (stopwatchTimer == null)
            {
                Debug.LogWarning("No se ha asignado StopwatchTimer en el checkpoint.");
                return;
            }

            // Obtener el tiempo actual del cronómetro
            float time = stopwatchTimer.GetElapsedTime();

            // Convertir a formato mm:ss:ff (minutos:segundos:centésimas)
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            int hundredths = Mathf.FloorToInt((time * 100) % 100);

            string formattedTime = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, hundredths);

            // Mostrar en el texto asignado
            if (checkpointTimeText != null)
                checkpointTimeText.text = formattedTime;
            else
                Debug.LogWarning("No se ha asignado el TextMeshProUGUI del checkpoint.");
        }
    }
}
