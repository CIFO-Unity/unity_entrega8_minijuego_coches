using UnityEngine;
using TMPro;

public class CarSpeedDisplay : MonoBehaviour
{
    [Header("UI Settings")]
    public TextMeshProUGUI speedText;   // Texto de la UI donde se mostrará la velocidad

    [Header("Update Settings")]
    public int updateEveryFrames = 5;   // Actualiza la velocidad cada X frames

    private PrometeoCarController carController;  // Referencia al script PrometeoCarController
    private int frameCounter = 0;                  // Contador de frames

    void Start()
    {
        // Buscar automáticamente el script PrometeoCarController en el mismo GameObject
        carController = GetComponent<PrometeoCarController>();

        if (carController == null)
        {
            Debug.LogError("No se encontró el componente PrometeoCarController en este GameObject.");
        }
    }

    void Update()
    {
        if (carController == null || speedText == null)
            return;

        frameCounter++;

        // Solo actualizar cada X frames
        if (frameCounter >= updateEveryFrames)
        {
            frameCounter = 0;

            // ⚠️ Ajusta el nombre de la variable de velocidad según tu PrometeoCarController
            float speed = carController.carSpeed;

            if (speed < 0)
                speed = 0;

            speedText.text = $"{speed:0} km/h";
        }
    }
}
