using UnityEngine;
using System.Collections.Generic;

public class CarPlayback : MonoBehaviour
{
    [Header("Playback Settings")]
    [SerializeField] private float playbackSpeed = 1f; // multiplicador de velocidad
    [SerializeField] private int recordEveryXFrames = 2; // debe coincidir con CarRecorder
    private float playbackInterval; // tiempo entre frames grabados
    private float playbackTimer = 0f;

    private List<Vector3> positions;
    private List<Quaternion> rotations;
    private int currentIndex = 0;
    private bool isPlaying = false;

    void Start()
    {
        // Calcular intervalo entre frames grabados
        playbackInterval = recordEveryXFrames * Time.fixedDeltaTime;
    }

    void FixedUpdate()
    {
        if (!isPlaying || positions == null || positions.Count == 0) return;

        playbackTimer += Time.fixedDeltaTime * playbackSpeed;

        // Avanzar índices según el tiempo transcurrido
        while (playbackTimer >= playbackInterval && currentIndex < positions.Count - 1)
        {
            currentIndex++;
            playbackTimer -= playbackInterval;
        }

        if (currentIndex < positions.Count)
        {
            // Interpolación lineal para posición y slerp para rotación
            Vector3 pos = Vector3.Lerp(
                positions[Mathf.Max(currentIndex - 1, 0)],
                positions[currentIndex],
                playbackTimer / playbackInterval
            );

            Quaternion rot = Quaternion.Slerp(
                rotations[Mathf.Max(currentIndex - 1, 0)],
                rotations[currentIndex],
                playbackTimer / playbackInterval
            );

            transform.position = pos;
            transform.rotation = rot;
        }
        else
        {
            // Termina la reproducción
            isPlaying = false;
            gameObject.SetActive(false);
        }
    }

    // --- Iniciar reproducción con una grabación ---
    public void StartPlayback(CarRecording recording)
    {
        if (recording == null || recording.frames.Count == 0)
        {
            //gameObject.SetActive(false); // no hay grabación
            return;
        }

        positions = new List<Vector3>();
        rotations = new List<Quaternion>();

        foreach (var frame in recording.frames)
        {
            positions.Add(new Vector3(frame.x, frame.y, frame.z));
            rotations.Add(new Quaternion(frame.rotX, frame.rotY, frame.rotZ, frame.rotW));
        }

        currentIndex = 0;
        playbackTimer = 0f;
        isPlaying = true;
        //gameObject.SetActive(true);
    }

    public void StopPlayback()
    {
        isPlaying = false;
        gameObject.SetActive(false);
    }
}
