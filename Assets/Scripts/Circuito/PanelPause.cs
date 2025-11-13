using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PanelPause : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject panel; // El panel de pausa
    [SerializeField] private Button playButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;

    public bool canPause = false; // <-- controla si se puede usar Escape
    private bool isPaused = false;

    void Start()
    {
        // Asegurarse de que el panel esté oculto al inicio
        if (panel != null)
            panel.SetActive(false);

        // Asignar eventos a los botones
        if (playButton != null)
            playButton.onClick.AddListener(ResumeGame);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartScene);

        if (exitButton != null)
            exitButton.onClick.AddListener(GoToMainMenu);

        canPause = false;
    }

    void Update()
    {
        // Detectar tecla Escape
        if (canPause && Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (panel != null)
            panel.SetActive(true);

        Time.timeScale = 0f; // Pausar el juego
        isPaused = true;
    }

    private void ResumeGame()
    {
        if (panel != null)
            panel.SetActive(false);

        Time.timeScale = 1f; // Reanudar el juego
        isPaused = false;
    }

    private void RestartScene()
    {
        SoundManager.SafeStopBackgroundMusic();
        Time.timeScale = 1f; // Asegurarse de que el tiempo vuelva a la normalidad
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GoToMainMenu()
    {
        SoundManager.SafeStopBackgroundMusic();
        Time.timeScale = 1f; // Asegurarse de que el tiempo vuelva a la normalidad
        SceneManager.LoadScene("MainMenu"); // Nombre de tu escena de menú
    }

    public void HidePlayButton()
    {
        if (playButton != null)
            playButton.gameObject.SetActive(false);
    }
}
