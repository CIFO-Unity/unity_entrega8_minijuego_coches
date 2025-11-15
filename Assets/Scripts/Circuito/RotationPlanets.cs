using UnityEngine;

public class RotationPlanets : MonoBehaviour
{
    [Header("Configuración de Rotación")]
    [Tooltip("Velocidad de rotación en el eje Y (horizontal)")]
    [SerializeField] private float rotationSpeed = 5f;

    [Tooltip("Si está activado, rota en sentido horario; si no, en antihorario")]
    [SerializeField] private bool sentidoHorario = true;

    private Vector3 rotationAxis;

    void Start()
    {
        // Gira en eje Y
        rotationAxis = Vector3.up;
    }

    void Update()
    {
        // Determina el sentido de rotación
        float direction = sentidoHorario ? 1f : -1f;

        // Rota el objeto lentamente en el eje Y con el sentido elegido
        transform.Rotate(rotationAxis, direction * rotationSpeed * Time.deltaTime, Space.World);
    }
}
