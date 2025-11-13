using UnityEngine;
using TMPro;

public class MainMenuBestTime : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textBestTime;

    void Start()
    {
        if (textBestTime == null)
        {
            Debug.LogWarning("No TextMeshProUGUI assigned to display best time.");
            return;
        }

        // Obtener el mejor tiempo guardado
        float bestTime = BestTimeManager.GetBestTime();

        if (bestTime != float.MaxValue)
        {
            // Convertir a minutos:segundos:centésimas
            int minutes = Mathf.FloorToInt(bestTime / 60f);
            int seconds = Mathf.FloorToInt(bestTime % 60f);
            int hundredths = Mathf.FloorToInt((bestTime * 100) % 100);

            textBestTime.text = string.Format("Best time: {0:00}:{1:00}:{2:00}", minutes, seconds, hundredths);
        }
        else
        {
            textBestTime.text = "Best time: --:--:--"; // No hay tiempo guardado
        }

        textBestTime.gameObject.SetActive(true);
    }
}
