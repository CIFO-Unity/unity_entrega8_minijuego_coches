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

    [Header("Stopwatch")]
    [SerializeField]
    private StopwatchTimer stopwatchTimer;      // Referencia al cronómetro

    [Header("Car Controller Activator")]
    [SerializeField]
    private CarControllerActivator carControllerActivator;

    [Header("Panel Pause")]
    [SerializeField]
    private PanelPause panelPause;

    [Header("Car Recorder")]
    [SerializeField]
    private CarRecorder carRecorder;
    [SerializeField]
    private CarPlayback carGhost;

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
        //SoundManager.SafePlaySound("3-2-1-Go");
        if (SoundManager.Instance != null)
            SoundManager.SafePlaySound("Three");

        StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        while (currentTime > 0)
        {
            // Reproducir sonido correspondiente al número
            switch (currentTime)
            {
                case 3:
                    if (SoundManager.Instance != null)
                        SoundManager.SafePlaySound("Three");
                    break;
                case 2:
                    if (SoundManager.Instance != null)
                        SoundManager.SafePlaySound("Two");
                    break;
                case 1:
                    if (SoundManager.Instance != null)
                        SoundManager.SafePlaySound("One");
                    break;
            }

            timerText.text = currentTime.ToString();

            yield return new WaitForSeconds(1f);
            currentTime--;
        }

        // Cuando llegue a 0, mostrar "Go!"
        timerText.text = "Go!";

        // Esperar 1 segundo antes de ocultarlo
        yield return new WaitForSeconds(1f);
        timerText.text = "";

        // Reproducir música de fondo usando SoundManager
        if (SoundManager.Instance != null)
            SoundManager.SafePlayBackgroundMusic("Nightcall");

        if (stopwatchTimer != null)
            stopwatchTimer.StartTimer();

        if (carControllerActivator != null)
            carControllerActivator.ActivateCarControl();

        // Permitir acceder el menú de pausa
        if (panelPause != null)
            panelPause.canPause = true;

        // Empezar a grabar el movimiento del coche
        if (carRecorder != null)
            carRecorder.StartRecording();

        // Reproducir el coche fantasma
        CarRecording recording = carRecorder.LoadRecording();
        carGhost.StartPlayback(recording);
    }
}
