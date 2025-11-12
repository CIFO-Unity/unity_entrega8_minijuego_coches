using UnityEngine;

public class FinishLine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StopwatchTimer stopwatchTimer;   // Referencia al cronómetro
    [SerializeField] private GameObject finishMessageUI;      // Objeto del Canvas (texto "Finish!")
    [SerializeField] private string playerTag = "Player";     // Tag del vehículo del jugador

    private void Start()
    {
        if (finishMessageUI != null)
            finishMessageUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // 1️⃣ Detener el cronómetro
            if (stopwatchTimer != null)
            {
                stopwatchTimer.StopTimer();
            }

            // 2️⃣ Detener el coche ajustando MaxSpeed a 0
            PrometeoCarController carController = other.GetComponent<PrometeoCarController>();
            if (carController != null)
            {
                carController.carSpeed = 0f;
                carController.enabled = false;
            }

            // 3️⃣ Mostrar el mensaje de "Finish"
            if (finishMessageUI != null)
            {
                finishMessageUI.SetActive(true);
            }

            Debug.Log("¡Meta alcanzada! Cronómetro detenido y vehículo parado.");
        }
    }
}
