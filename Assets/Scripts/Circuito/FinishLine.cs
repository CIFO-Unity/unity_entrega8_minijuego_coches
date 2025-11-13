using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FinishLine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StopwatchTimer stopwatchTimer;   // Cronómetro
    [SerializeField] private GameObject finishMessageUI;      // Texto "Finish!"
    [SerializeField] private string playerTag = "Player";     // Tag del vehículo
    [SerializeField] private Image finishPanel;              // Panel que se desvanecerá al llegar a meta

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 2f;         // Duración del fade
    [SerializeField] private float targetAlpha = 0.7f;        // Alfa máximo (0 a 1)

    private void Start()
    {
        if (finishMessageUI != null)
            finishMessageUI.SetActive(false);

        // Asegurarse de que el panel empieza invisible
        if (finishPanel != null)
        {
            Color c = finishPanel.color;
            c.a = 0f;
            finishPanel.color = c;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // 1️⃣ Detener el cronómetro
            if (stopwatchTimer != null)
                stopwatchTimer.StopTimer();

            // 2️⃣ Detener el coche ajustando MaxSpeed a 0
            PrometeoCarController carController = other.GetComponent<PrometeoCarController>();
            if (carController != null)
            {
                carController.carSpeed = 0f;
                carController.enabled = false;
            }

            // 3️⃣ Mostrar el mensaje de "Finish"
            if (finishMessageUI != null)
                finishMessageUI.SetActive(true);

            // 4️⃣ Iniciar el fade del panel
            if (finishPanel != null)
                StartCoroutine(FadeInPanel(finishPanel, fadeDuration, targetAlpha));

            // Fade out de la música de fondo
            StartCoroutine(SoundManager.Instance.FadeOutMusic(fadeDuration));

            Debug.Log("¡Meta alcanzada! Cronómetro detenido y vehículo parado.");
        }
    }

    private IEnumerator FadeInPanel(Image panel, float duration, float targetAlpha)
    {
        float elapsed = 0f;
        Color color = panel.color;
        float startAlpha = color.a;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            color.a = alpha;
            panel.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        panel.color = color;
    }
}
