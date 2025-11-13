using UnityEngine;
using TMPro;
using System.Collections;

public class CountdownTimer : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField]
    private TextMeshProUGUI timerText;  // Texto donde se mostrará la cuenta atrás

    [Header("Countdown Settings")]
    [SerializeField]
    private int startTime = 10;         // Tiempo inicial en segundos

    [SerializeField]
    [Header("Stopwatch")]
    private StopwatchTimer stopwatchTimer;      // Referencia al cronómetro

    [SerializeField]
    [Header("Car Controller Activator")]
    private CarControllerActivator carControllerActivator;

    [SerializeField]
    [Header("Panel Pause")]
    private PanelPause panelPause;

    private int currentTime;

    void Start()
    {
        if (timerText == null)
        {
            Debug.LogError("No se ha asignado TextMeshProUGUI en el inspector.");
            return;
        }

        currentTime = startTime;
        timerText.text = currentTime.ToString();

        // Reproducir sonido usando SoundManager
        SoundManager.SafePlaySound("3-2-1-Go");

        StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        while (currentTime > 0)
        {
            yield return new WaitForSeconds(1f);
            currentTime--;
            timerText.text = currentTime.ToString();
        }

        // Cuando llegue a 0, mostrar "Go!"
        timerText.text = "Go!";

        // Esperar 1 segundo antes de ocultarlo
        yield return new WaitForSeconds(1f);
        timerText.text = "";

        // Reproducir música de fondo usando SoundManager
        SoundManager.SafePlayBackgroundMusic("Nightcall");

        if (stopwatchTimer != null)
            stopwatchTimer.StartTimer();

        if (carControllerActivator != null)
            carControllerActivator.ActivateCarControl();

        // Permitir acceder el menú de pausa
        if (panelPause != null)
            panelPause.canPause = true;
    }
}
