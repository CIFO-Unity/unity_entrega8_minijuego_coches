using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class CountdownTimer : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Stopwatch")]
    [SerializeField] private StopwatchTimer stopwatchTimer;

    [Header("Car Controller Activator")]
    [SerializeField] private CarControllerActivator carControllerActivator;

    [Header("Panel Pause")]
    [SerializeField] private PanelPause panelPause;

    [Header("Car Recorder & Ghost")]
    [SerializeField] private CarRecorder carRecorder;
    [SerializeField] private CarPlayback carGhost;

    [Header("Traffic Lights")]
    [SerializeField] private GameObject redLight;
    [SerializeField] private GameObject yellowLight;
    [SerializeField] private GameObject greenLight;

    [Header("Audio Clips for Lights")]
    [SerializeField] private string redLightSound = "CuentaAtras";
    [SerializeField] private string yellowLightSound = "CuentaAtras";
    [SerializeField] private string greenLightSound = "CuentaAtrasFinal";

    [Header("Countdown Settings")]
    [SerializeField] private float interval = 1f; // segundos entre cada luz

    void Start()
    {
        // Apagar todas las luces al inicio
        SetAllLights(false);

        // Iniciar la secuencia del semáforo
        StartCoroutine(CountdownSequence());
    }

    private IEnumerator CountdownSequence()
    {
        // Esperar 1 segundo antes de encender la primera luz
        yield return new WaitForSeconds(1f);

        // 1️⃣ Encender rojas y reproducir sonido
        ActivateRed();
        if (SoundManager.Instance != null)
            SoundManager.SafePlaySound(redLightSound);

        yield return new WaitForSeconds(interval);

        // 2️⃣ Encender amarillas (rojas permanecen) y reproducir sonido
        ActivateYellow();
        if (SoundManager.Instance != null)
            SoundManager.SafePlaySound(yellowLightSound);

        yield return new WaitForSeconds(interval);

        // 3️⃣ Encender verdes (rojas y amarillas permanecen) y reproducir sonido
        ActivateGreen();
        if (SoundManager.Instance != null)
            SoundManager.SafePlaySound(greenLightSound);

        // Mostrar "Go!" en el UI
        /*if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = "Go!";
        }*/

        // Permitir que el jugador se mueva
        // Llamar a todos los CarControllerActivator que existan en la escena (si hay varios)
        CarControllerActivator[] activators = null;
        if (carControllerActivator != null)
        {
            activators = new CarControllerActivator[] { carControllerActivator };
            Debug.Log($"CountdownTimer: using serialized CarControllerActivator on '{carControllerActivator.gameObject.name}'.");
        }
        else
        {
            activators = FindObjectsOfType<CarControllerActivator>(true);
        }

        if (activators != null)
        {
            foreach (var act in activators)
            {
                if (act == null) continue;
                act.ActivateCarControl();
            }
        }

        // Iniciar cronómetro
        stopwatchTimer?.StartTimer();

        // Permitir menú de pausa
        if (panelPause != null)
            panelPause.canPause = true;

        // Empezar a grabar el coche del jugador
        carRecorder?.StartRecording();

        // Reproducir el coche fantasma si hay grabación previa
        CarRecording recording = carRecorder?.LoadRecording();
        if (recording != null)
            carGhost.StartPlayback(recording);

        // Esperar 1 segundo antes de empezar la música de fondo
        yield return new WaitForSeconds(0.75f);
        if (SoundManager.Instance != null)
        {
            string sceneName = SceneManager.GetActiveScene().name;

            switch (sceneName)
            {
                case "Circuito":
                    SoundManager.SafePlayBackgroundMusic("Nightcall");
                    break;

                case "Circuito_2":
                    SoundManager.SafePlayBackgroundMusic("ARealHero");
                    break;

                case "Circuito_4":
                    SoundManager.SafePlayBackgroundMusic("UnderYourSpell");
                    break;

                default:
                    SoundManager.SafePlayBackgroundMusic("Nightcall");
                    break;
            }
        }

        // Esperar un segundo antes de ocultar el texto
        yield return new WaitForSeconds(1f);
        if (timerText != null)
            timerText.text = "";
    }

    // --- Métodos auxiliares para luces ---
    private void SetAllLights(bool state)
    {
        if (redLight != null) redLight.SetActive(state);
        if (yellowLight != null) yellowLight.SetActive(state);
        if (greenLight != null) greenLight.SetActive(state);
    }

    private void ActivateRed()
    {
        if (redLight != null) redLight.SetActive(true);
        if (yellowLight != null) yellowLight.SetActive(false);
        if (greenLight != null) greenLight.SetActive(false);
    }

    private void ActivateYellow()
    {
        if (redLight != null) redLight.SetActive(true);
        if (yellowLight != null) yellowLight.SetActive(true);
        if (greenLight != null) greenLight.SetActive(false);
    }

    private void ActivateGreen()
    {
        if (redLight != null) redLight.SetActive(true);
        if (yellowLight != null) yellowLight.SetActive(true);
        if (greenLight != null) greenLight.SetActive(true);
    }
}
