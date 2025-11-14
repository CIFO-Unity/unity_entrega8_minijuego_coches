using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Botones")]
    [SerializeField] private Button buttonCircuito;
    [SerializeField] private Button buttonCircuito2P;
    [SerializeField] private Button buttonCircuito4P;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1.5f; // duración del fade de música

    void Start()
    {
        if (buttonCircuito != null)
            buttonCircuito.onClick.AddListener(() => StartCoroutine(LoadSceneWithFade("Circuito")));

        if (buttonCircuito2P != null)
            buttonCircuito2P.onClick.AddListener(() => StartCoroutine(LoadSceneWithFade("Circuito_2")));

        if (buttonCircuito4P != null)
            buttonCircuito4P.onClick.AddListener(() => StartCoroutine(LoadSceneWithFade("Circuito_4")));
    }

    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        // Llamar a fade out de la música
        if (SoundManager.Instance != null)
            StartCoroutine(SoundManager.Instance.FadeOutMusic(fadeDuration));
            //SoundManager.Instance.StopBackgroundMusic();

        // Esperar la duración del fade antes de cambiar de escena
        yield return new WaitForSeconds(fadeDuration);

        SceneManager.LoadScene(sceneName);
    }
}
