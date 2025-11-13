using UnityEngine;
using System.Collections.Generic;

public class CarPlayback : MonoBehaviour
{
    [Header("Playback")]
    [SerializeField] private float playbackSpeed = 1f;
    private List<Vector3> positions;
    private List<Quaternion> rotations;
    private int currentIndex = 0;
    private bool isPlaying = false;

    void FixedUpdate()
    {
        if (!isPlaying || positions == null || positions.Count == 0) return;

        transform.position = positions[currentIndex];
        transform.rotation = rotations[currentIndex];

        currentIndex += Mathf.RoundToInt(playbackSpeed);

        if (currentIndex >= positions.Count)
        {
            isPlaying = false;
            gameObject.SetActive(false); // Ocultar al terminar
        }
    }

    public void StartPlayback(CarRecording recording)
    {
        if (recording == null || recording.frames.Count == 0)
        {
            gameObject.SetActive(false); // No hay grabación, ocultar coche fantasma
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
        isPlaying = true;
        gameObject.SetActive(true);
    }

    public void StopPlayback()
    {
        isPlaying = false;
        gameObject.SetActive(false);
    }
}
