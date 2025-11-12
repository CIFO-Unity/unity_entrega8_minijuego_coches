using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SplashScreenController : MonoBehaviour
{
    [SerializeField] private Image fadePanel;
    [SerializeField] private float waitTime = 3.0f;
    [SerializeField] private float fadeDuration = 1.5f;

    void Start()
    {
        // Iniciar con pantalla transparente
        Color color = fadePanel.color;
        color.a = 0f;
        fadePanel.color = color;
        
        Invoke("StartFade", waitTime);
    }

    private void StartFade()
    {
        StartCoroutine(FadeOut());
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

        // Pequeña pausa antes de cambiar de escena
        yield return new WaitForSeconds(0.2f);
        
        SceneManager.LoadScene("MainMenu");
    }
}
