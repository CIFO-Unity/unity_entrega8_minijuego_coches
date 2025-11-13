using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private Transform musicParent; // Auto-asignado al hijo "Music"
    private Transform sfxParent;   // Auto-asignado al hijo "SFX"

    private AudioSource currentMusicSource;


    #region Awake

    void Awake()
    {
        // Singleton

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);


            // Buscar automáticamente los hijos Music y SFX

            musicParent = transform.Find("Music");
            sfxParent = transform.Find("SFX");

            if (musicParent == null)
                Debug.LogWarning("No child named 'Music' found under SoundManager.");
            if (sfxParent == null)
                Debug.LogWarning("No child named 'SFX' found under SoundManager.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion

    #region Música de fondo

    public void PlayBackgroundMusic(string musicName)
    {
        if (musicParent == null)
            return;


        Transform musicObj = musicParent.Find(musicName);

        if (musicObj == null)
        {
            Debug.LogWarning("Music object not found: " + musicName);
            return;
        }


        AudioSource musicSource = musicObj.GetComponent<AudioSource>();

        if (musicSource == null)
        {
            Debug.LogWarning("No AudioSource found on " + musicName);
            return;
        }


        // Evitar reiniciar la misma música

        if (currentMusicSource == musicSource && musicSource.isPlaying) return;


        // Detener música anterior

        if (currentMusicSource != null)
            currentMusicSource.Stop();


        // Reproducir nueva música en loop

        currentMusicSource = musicSource;
        currentMusicSource.loop = true;

        /*if(musicName == "GameOver")
            currentMusicSource.loop = false;*/ // Esta pista no debe reproducirse en loop

        currentMusicSource.Play();
    }

    public void StopBackgroundMusic()
    {
        if (currentMusicSource != null && currentMusicSource.isPlaying)
        {
            currentMusicSource.Stop();
        }
    }

    public void PauseBackgroundMusic()
    {
        if (currentMusicSource != null && currentMusicSource.isPlaying)
        {
            currentMusicSource.Pause();
        }
    }

    public void ResumeBackgroundMusic()
    {
        if (currentMusicSource != null && !currentMusicSource.isPlaying)
        {
            currentMusicSource.UnPause();
        }
    }

    public IEnumerator FadeOutMusic(float duration = 1.5f)
    {
        if (currentMusicSource == null || !currentMusicSource.isPlaying)
            yield break;

        float startVolume = currentMusicSource.volume;

        while (currentMusicSource.volume > 0f)
        {
            currentMusicSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        currentMusicSource.Stop();
        currentMusicSource.volume = startVolume; // Restaurar volumen para la próxima vez
    }

    #endregion

    #region Efectos de sonido

    public void PlaySound(string sfxName)
    {
        if (sfxParent == null)
            return;


        Transform sfxObj = sfxParent.Find(sfxName);

        if (sfxObj == null)
        {
            Debug.LogWarning("SFX object not found: " + sfxName);
            return;
        }


        AudioSource sfxSource = sfxObj.GetComponent<AudioSource>();

        if (sfxSource == null)
        {
            Debug.LogWarning("No AudioSource found on " + sfxName);
            return;
        }


        // Reproducir el efecto de sonido

        sfxSource.PlayOneShot(sfxSource.clip);
    }

    #endregion

    #region Mejoras futuras

    public static void SafePlayBackgroundMusic(string musicName)
    {
        if (Instance == null)
        {
            Debug.LogWarning("SoundManager not initialized. Cannot play background music: " + musicName);
            return;
        }

        Instance.PlayBackgroundMusic(musicName);
    }
    
    public static void SafeStopBackgroundMusic()
    {
        if (Instance == null)
        {
            Debug.LogWarning("SoundManager not initialized. Cannot stop background music.");
            return;
        }

        Instance.StopBackgroundMusic();
    }

    public static void SafePlaySound(string sfxName)
    {
        if (Instance == null)
        {
            Debug.LogWarning("SoundManager not initialized. Cannot play sound: " + sfxName);
            return;
        }

        Instance.PlaySound(sfxName);
    }

    #endregion
}
