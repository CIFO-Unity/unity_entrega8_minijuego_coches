using UnityEngine;
using TMPro;

public class StopwatchTimer : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("One or more UI text elements where the timer will be displayed. If multiple, the same time is written to each.")]
    [SerializeField] public TextMeshProUGUI[] timerText; // Text elements where the time will be displayed

    private float elapsedTime = 0f;   // Tiempo acumulado
    private bool isRunning = false;   // Ahora empieza detenido

    void Update()
    {
        if (!isRunning || timerText == null || timerText.Length == 0)
            return;

        // Incrementar el tiempo
        elapsedTime += Time.deltaTime;

        // Mostrar en el texto, formato mm:ss:ff (minutos:segundos:centésimas)
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int hundredths = Mathf.FloorToInt((elapsedTime * 100) % 100);

        string formatted = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, hundredths);
        for (int i = 0; i < timerText.Length; i++)
        {
            if (timerText[i] == null) continue;
            timerText[i].text = formatted;
        }
    }

    /// <summary>
    /// Inicia o reanuda el cronómetro
    /// </summary>
    public void StartTimer()
    {
        isRunning = true;
    }

    /// <summary>
    /// Detiene el cronómetro
    /// </summary>
    public void StopTimer()
    {
        isRunning = false;
    }

    /// <summary>
    /// Reinicia el cronómetro y empieza desde cero
    /// </summary>
    public void ResetTimer()
    {
        elapsedTime = 0f;
        isRunning = true;
    }

    /// <summary>
    /// Reinicia el cronómetro pero lo deja detenido
    /// </summary>
    public void ResetAndStop()
    {
        elapsedTime = 0f;
        isRunning = false;
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }
}
