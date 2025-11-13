using UnityEngine;
using System.Collections;

public class CheckpointLightsBlinker : MonoBehaviour
{
    [Header("Luces del checkpoint")]
    [SerializeField] private Light[] spotlights; // Arrastra aquí tus luces

    [Header("Configuración del parpadeo")]
    [SerializeField] private float blinkInterval = 0.5f; // Tiempo entre encendido y apagado
    [SerializeField] private bool startOnAwake = true;   // Iniciar automáticamente
    [SerializeField] private bool randomOffset = false;  // Parpadeo asincrónico

    private bool isBlinking = false;

    void Start()
    {
        if (startOnAwake)
            StartBlinking();
    }

    public void StartBlinking()
    {
        if (!isBlinking && spotlights.Length > 0)
            StartCoroutine(BlinkCoroutine());
    }

    public void StopBlinking()
    {
        isBlinking = false;
        StopAllCoroutines();

        // Dejar todas las luces encendidas al terminar
        foreach (var light in spotlights)
        {
            if (light != null)
                light.enabled = true;
        }
    }

    private IEnumerator BlinkCoroutine()
    {
        isBlinking = true;

        while (isBlinking)
        {
            foreach (var light in spotlights)
            {
                if (light != null)
                    light.enabled = !light.enabled;
            }

            float waitTime = randomOffset ? blinkInterval + Random.Range(-0.1f, 0.1f) : blinkInterval;
            yield return new WaitForSeconds(waitTime);
        }
    }
}
