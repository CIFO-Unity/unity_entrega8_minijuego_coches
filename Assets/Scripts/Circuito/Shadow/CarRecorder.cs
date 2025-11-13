using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

[Serializable]
public class CarFrameData
{
    public float x, y, z;
    public float rotX, rotY, rotZ, rotW;
}

[Serializable]
public class CarRecording
{
    public float completionTime; // tiempo total del circuito
    public List<CarFrameData> frames = new List<CarFrameData>();
}

public class CarRecorder : MonoBehaviour
{
    [Header("Grabación")]
    [SerializeField] private int recordEveryXFrames = 2; // cada cuántos frames grabar
    private bool isRecording = false;
    private float elapsedTime = 0f;

    private List<Vector3> positions = new List<Vector3>();
    private List<Quaternion> rotations = new List<Quaternion>();
    private int frameCounter = 0;

    private string recordingFileName = "CarRecording.json";

    // --- Métodos públicos para controlar la grabación ---
    public void StartRecording()
    {
        positions.Clear();
        rotations.Clear();
        frameCounter = 0;
        elapsedTime = 0f;
        isRecording = true;
        Debug.Log("Grabación iniciada");
    }

    public void StopRecording(float totalTime)
    {
        isRecording = false;
        SaveRecording(totalTime);
        Debug.Log("Grabación detenida");
    }

    // --- FixedUpdate graba posiciones cada X frames ---
    void FixedUpdate()
    {
        if (!isRecording) return;

        frameCounter++;
        if (frameCounter >= recordEveryXFrames)
        {
            frameCounter = 0;
            positions.Add(transform.position);
            rotations.Add(transform.rotation);
        }

        elapsedTime += Time.fixedDeltaTime;
    }

    // --- Guardar la grabación en disco ---
    private void SaveRecording(float completionTime)
    {
        CarRecording recording = new CarRecording();
        recording.completionTime = completionTime;

        for (int i = 0; i < positions.Count; i++)
        {
            CarFrameData frame = new CarFrameData
            {
                x = positions[i].x,
                y = positions[i].y,
                z = positions[i].z,
                rotX = rotations[i].x,
                rotY = rotations[i].y,
                rotZ = rotations[i].z,
                rotW = rotations[i].w
            };
            recording.frames.Add(frame);
        }

        string path = Application.persistentDataPath + "/" + recordingFileName;

        //Debug.Log("El archivo se guarda en: " + Application.persistentDataPath);

        // Si ya hay una grabación, solo sustituir si el tiempo es mejor
        if (File.Exists(path))
        {
            string existingJson = File.ReadAllText(path);
            CarRecording existingRecording = JsonUtility.FromJson<CarRecording>(existingJson);

            if (completionTime >= existingRecording.completionTime)
            {
                Debug.Log("Tiempo no mejor que la grabación anterior, no se guarda");
                return;
            }
        }

        string json = JsonUtility.ToJson(recording, true);
        File.WriteAllText(path, json);
        Debug.Log("Grabación guardada en: " + path);
    }

    // --- Cargar grabación desde disco ---
    public CarRecording LoadRecording()
    {
        string path = Application.persistentDataPath + "/" + recordingFileName;
        if (!File.Exists(path))
        {
            Debug.Log("No hay grabación previa en disco");
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<CarRecording>(json);
    }
}
