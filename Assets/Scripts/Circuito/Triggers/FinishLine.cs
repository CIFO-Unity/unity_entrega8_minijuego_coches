using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class FinishLine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StopwatchTimer stopwatchTimer;   // Cronómetro
    [SerializeField] private GameObject finishMessageUI;      // Texto "Finish!"
    [SerializeField] private string playerTag = "Player";     // Tag del vehículo
    [SerializeField] private Image finishPanel;              // Panel que se desvanecerá al llegar a meta
    [SerializeField] private TextMeshProUGUI textYourTime;
    [SerializeField] private TextMeshProUGUI textBestTime; 
    [SerializeField] private PanelPause panelPauseScript; // Referencia al script PanelPause
    [SerializeField] private float delayPanelPause = 5f; // Segundos antes de mostrar el panel
    [SerializeField] private CarRecorder carRecorder; 
    [SerializeField] private CheckpointCounter checkPointCounter; 


    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 2f;         // Duración del fade
    [SerializeField] private float targetAlpha = 0.7f;        // Alfa máximo (0 a 1)

    [Header("Blink Settings")]
    [SerializeField] private Color blinkColor = Color.red;
    [SerializeField] private float duration = 2f;
    [SerializeField] private float blinkSpeed = 0.2f; // tiempo entre cambios de color

    private void Start()
    {
        if (finishMessageUI != null)
            finishMessageUI.SetActive(false);

        if (textYourTime != null)
            textYourTime.gameObject.SetActive(false);

        if (textBestTime != null)
            textBestTime.gameObject.SetActive(false);

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
            if(checkPointCounter != null)
            {
                if(checkPointCounter.GetCheckpointCount() >= 3)
                {
                    // 1️⃣ Detener el cronómetro
                    if (stopwatchTimer != null)
                        stopwatchTimer.StopTimer();

                    if (carRecorder != null & stopwatchTimer != null)
                        carRecorder.StopRecording(stopwatchTimer.GetElapsedTime());

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
                    {
                        finishPanel.gameObject.SetActive(true);
                        StartCoroutine(FadeInPanel(finishPanel, fadeDuration, targetAlpha));
                    }

                    // Fade out de la música de fondo
                    StartCoroutine(SoundManager.Instance.FadeOutMusic(fadeDuration));

                    // Guardar el tiempo sólo si es mejor que el ya guardado
                    BestTimeManager.SaveBestTime(stopwatchTimer.GetElapsedTime());

                    // Mostrar tu tiempo
                    if (textYourTime != null && stopwatchTimer != null)
                    {
                        // Mostrar en el texto, formato mm:ss:ff (minutos:segundos:centésimas)
                        int minutes = Mathf.FloorToInt(stopwatchTimer.GetElapsedTime() / 60f);
                        int seconds = Mathf.FloorToInt(stopwatchTimer.GetElapsedTime() % 60f);
                        int hundredths = Mathf.FloorToInt((stopwatchTimer.GetElapsedTime() * 100) % 100);

                        textYourTime.text = string.Format("Your time: {0:00}:{1:00}:{2:00}", minutes, seconds, hundredths);

                        if (textYourTime != null)
                            textYourTime.gameObject.SetActive(true);
                    }

                    // Mostrar mejor tiempo
                    if (textBestTime != null && stopwatchTimer != null)
                    {
                        float bestTime = BestTimeManager.GetBestTime();

                        // Verificar si hay un tiempo válido
                        if (bestTime != float.MaxValue)
                        {
                            // Convertir a minutos, segundos y centésimas
                            int minutes = Mathf.FloorToInt(bestTime / 60f);
                            int seconds = Mathf.FloorToInt(bestTime % 60f);
                            int hundredths = Mathf.FloorToInt((bestTime * 100) % 100);

                            textBestTime.text = string.Format("Best time: {0:00}:{1:00}:{2:00}", minutes, seconds, hundredths);

                            // Activar el texto si estaba oculto
                            textBestTime.gameObject.SetActive(true);

                            // Hacer que parpadee si se ha conseguido un nuevo récord
                            if (stopwatchTimer.GetElapsedTime() <= bestTime)
                                StartCoroutine(BlinkCoroutine());
                        }
                        else
                        {
                            // No hay tiempo guardado todavía
                            textBestTime.text = "Best time: --:--:--";
                            textBestTime.gameObject.SetActive(true);
                        }
                    }
                    
                    if (panelPauseScript != null)
                    {
                        // Evitar que el usuario abra el menú de Pausa cuando se ha acabado la carrera
                        panelPauseScript.canPause = false;

                        // Iniciar la corutina que lo activará después del delay
                        StartCoroutine(ActivatePausePanelAfterDelay());
                    }

                    Debug.Log("¡Meta alcanzada! Cronómetro detenido y vehículo parado.");
                }
            }
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

    private IEnumerator BlinkCoroutine()
    {
        Color originalColor = textBestTime.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Alternar entre color original y color de parpadeo
            textBestTime.color = textBestTime.color == originalColor ? blinkColor : originalColor;

            elapsed += blinkSpeed;
            yield return new WaitForSeconds(blinkSpeed);
        }

        // Asegurarse de que el texto vuelve al color original
        textBestTime.color = originalColor;
    }

    private IEnumerator ActivatePausePanelAfterDelay()
    {
        yield return new WaitForSeconds(delayPanelPause);

        // Activar panel y ocultar el botón Play
        panelPauseScript.HidePlayButton();
        panelPauseScript.PauseGame();
    }
}
