using UnityEngine;

public class CarControllerActivator : MonoBehaviour
{
    [SerializeField] private PrometeoCarController carController; // Referencia al script de control del coche

    private void Start()
    {
        if (carController == null)
        {
            carController = GetComponent<PrometeoCarController>();
        }

        if (carController != null)
        {
            carController.enabled = false; // Desactivar el control al inicio
        }
        else
        {
            Debug.LogWarning("No se encontró el componente PrometeoCarController en el GameObject del vehículo.");
        }
    }

    /// <summary>
    /// Activa el control del vehículo.
    /// </summary>
    public void ActivateCarControl()
    {
        if (carController != null)
        {
            carController.enabled = true;
        }
    }

    /// <summary>
    /// Desactiva el control del vehículo.
    /// </summary>
    public void DeactivateCarControl()
    {
        if (carController != null)
        {
            carController.enabled = false;
        }
    }
}
