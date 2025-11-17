using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Fade Panel")]
    [SerializeField] private Image fadePanel;


    [Header("Botones")]

    [SerializeField] private Button buttonSelectCar;
    [SerializeField] private Button buttonCircuito;
    [SerializeField] private Button buttonCircuito2P;
    [SerializeField] private Button buttonCircuito4P;



    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1.5f; // duración del fade out y música
    [SerializeField] private float fadeInDuration = 3.5f; // duración del fade in al iniciar

    void Awake()
    {
        // CRÍTICO: Establecer pantalla completamente negra ANTES del primer frame
        if (fadePanel != null)
        {
            Color color = fadePanel.color;
            color.a = 1f;
            fadePanel.color = color;
            fadePanel.raycastTarget = true; // Bloquear clics durante el fade in
        }
    }

    void Start()
    {
        // Iniciar el fade in después de que la escena esté completamente cargada
        StartCoroutine(FadeInRoutine());

        if (buttonSelectCar != null)
            buttonSelectCar.onClick.AddListener(() => StartCoroutine(LoadSceneWithFade("SelectCar")));


        if (buttonCircuito != null)
            buttonCircuito.onClick.AddListener(() => StartCoroutine(LoadSceneWithFade("Circuito")));

        if (buttonCircuito2P != null)
            buttonCircuito2P.onClick.AddListener(() => StartCoroutine(LoadSceneWithFade("Circuito_2")));

        if (buttonCircuito4P != null)
            buttonCircuito4P.onClick.AddListener(() => StartCoroutine(LoadSceneWithFade("Circuito_4")));
    }
    
    private IEnumerator FadeInRoutine()
    {
        // Pausa para asegurar que la escena está completamente cargada
        yield return new WaitForSeconds(0.5f);
        
        yield return StartCoroutine(FadeIn());
    }

    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        Debug.Log("LoadSceneWithFade iniciado");
        
        // Reactivar raycast para bloquear clics durante el fade out
        if (fadePanel != null)
            fadePanel.raycastTarget = true;
        
        // Iniciar fade out de la música EN PARALELO con el fade visual
        if (SoundManager.Instance != null)
            StartCoroutine(SoundManager.Instance.FadeOutMusic(fadeDuration));

        // Fade out del panel (de transparente a negro) - dura fadeDuration
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);

            Color color = fadePanel.color;
            color.a = alpha;
            fadePanel.color = color;
            
            Debug.Log($"Fade progress: alpha={alpha:F2}");
            yield return null;
        }

        // Asegurar que termina en negro completo
        Color finalColor = fadePanel.color;
        finalColor.a = 1f;
        fadePanel.color = finalColor;
        
        Debug.Log("Fade completo - pantalla negra");

        // Pequeña pausa para que se vea el negro completo
        yield return new WaitForSeconds(1.5f);

        // Cargar escena después de que el fade esté completo
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;

        // De negro opaco (1) a transparente (0)
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeInDuration);

            Color color = fadePanel.color;
            color.a = alpha;
            fadePanel.color = color;

            yield return null;
        }

        // Asegurar que termina transparente
        Color finalColor = fadePanel.color;
        finalColor.a = 0f;
        fadePanel.color = finalColor;
        
        // Desactivar raycast para permitir clics en botones
        if (fadePanel != null)
            fadePanel.raycastTarget = false;
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;

        // De transparente (0) a negro opaco (1)
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);

            Color color = fadePanel.color;
            color.a = alpha;
            fadePanel.color = color;

            yield return null;
        }

        // Asegurar que termina en negro completo
        Color finalColor = fadePanel.color;
        finalColor.a = 1f;
        fadePanel.color = finalColor;
    }
}
