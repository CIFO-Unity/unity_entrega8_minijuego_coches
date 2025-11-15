using UnityEngine;

public class RotationWheel : MonoBehaviour
{
    [Header("Configuración de Rotación")]
    [Tooltip("Velocidad de rotación en el eje X (vertical)")]
    [SerializeField] private float rotationSpeed = 5f;

    [Tooltip("Si está activado, rota en sentido horario; si no, en antihorario")]
    [SerializeField] private bool sentidoHorario = true;

    private Vector3 rotationAxis;
    private float currentXAngle;
    private float baseYAngle;
    private float baseZAngle;

    void Start()
    {
        // Gira en eje X
        rotationAxis = Vector3.right;
        // Inicializar ángulo acumulado desde la rotación local actual
        var e = transform.localEulerAngles;
        currentXAngle = e.x;
        baseYAngle = e.y;
        baseZAngle = e.z;
    }

    void Update()
    {
        // Determina el sentido de rotación
        float direction = sentidoHorario ? 1f : -1f;

        // Acumular ángulo X y asignarlo (uso Repeat para mantenerlo en 0..360)
        float delta = direction * rotationSpeed * Time.deltaTime;
        currentXAngle += delta;
        currentXAngle = Mathf.Repeat(currentXAngle, 360f);

        // Aplicar rotación forzando solo la componente X; conservar Y/Z base para evitar
        // que Unity convierta a quaternions con componentes inesperadas en otros ejes.
        transform.localRotation = Quaternion.Euler(currentXAngle, baseYAngle, baseZAngle);
    }
}
