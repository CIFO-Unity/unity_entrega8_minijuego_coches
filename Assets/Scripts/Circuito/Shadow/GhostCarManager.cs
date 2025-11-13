using UnityEngine;

public class GhostCarManager : MonoBehaviour
{
    [SerializeField] public CarPlayback carGhost;
    [SerializeField] public CarRecorder carRecorder;

    void Start()
    {
        ShowGhostIfRecordingExists();
    }

    public void ShowGhostIfRecordingExists()
    {
        if (carRecorder == null || carGhost == null)
            return;

        // Cargar la grabación desde disco
        var recording = carRecorder.LoadRecording();

        if (recording == null || recording.frames.Count == 0)
        {
            // No hay grabación, ocultar ghost
            carGhost.gameObject.SetActive(false);
        }
        else
        {
            // Hay grabación, mostrar ghost
            carGhost.gameObject.SetActive(true);
        }
    }
}
