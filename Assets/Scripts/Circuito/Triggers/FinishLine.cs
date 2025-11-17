using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class FinishLineMultiplayer : MonoBehaviour
{
    [System.Serializable]
    public class PlayerFinishUI
    {
        public string playerTag;                    // "Player", "Player2", "Player3", "Player4"
        public TextMeshProUGUI finishMessageUI;     // Texto para mostrar posición ("1st Place!", "2nd Place", etc.)
        public Image finishPanel;                   // Panel individual que se desvanecerá
        public TextMeshProUGUI textYourTime;
        public TextMeshProUGUI textBestTime;
        public StopwatchTimer stopwatchTimer;       // Cronómetro individual
        public CheckpointCounter checkpointCounter; // Contador de checkpoints individual
        public CarRecorder carRecorder;             // Grabador individual (opcional)
    }

    [Header("Players Setup")]
    [SerializeField] private List<PlayerFinishUI> players = new List<PlayerFinishUI>();

    [Header("Global Settings")]
    [SerializeField] private PanelPause panelPauseScript;
    [SerializeField] private float delayPanelPause = 5f;

    [Header("Particles")]
    [Tooltip("Sistemas de partículas a activar al cruzar la meta (bengalas, cohetes, etc.)")]
    [SerializeField] private ParticleSystem[] finishParticles;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float targetAlpha = 0.7f;

    [Header("Blink Settings")]
    [SerializeField] private Color blinkColor = Color.red;
    [SerializeField] private float blinkDuration = 2f;
    [SerializeField] private float blinkSpeed = 0.2f;

    [Header("Position Colors")]
    [SerializeField] private Color firstPlaceColor = Color.yellow;
    [SerializeField] private Color secondPlaceColor = new Color(0.75f, 0.75f, 0.75f); // Plata
    [SerializeField] private Color thirdPlaceColor = new Color(0.8f, 0.5f, 0.2f);     // Bronce
    [SerializeField] private Color fourthPlaceColor = Color.white;

    private HashSet<string> finishedPlayers = new HashSet<string>();
    private int totalPlayers = 0;
    private int finishPosition = 1;

    private void Start()
    {
        // Contar jugadores activos en la escena
        CountActivePlayers();

        // Inicializar UI de cada jugador
        foreach (var player in players)
        {
            if (player.finishMessageUI != null)
                player.finishMessageUI.gameObject.SetActive(false);

            if (player.textYourTime != null)
                player.textYourTime.gameObject.SetActive(false);

            if (player.textBestTime != null)
                player.textBestTime.gameObject.SetActive(false);

            if (player.finishPanel != null)
            {
                Color c = player.finishPanel.color;
                c.a = 0f;
                player.finishPanel.color = c;
            }
        }
    }

    private void CountActivePlayers()
    {
        // Detectar cuántos jugadores hay en la escena
        string[] possibleTags = { "Player", "Player2", "Player3", "Player4" };
        totalPlayers = 0;

        foreach (string tag in possibleTags)
        {
            try
            {
                GameObject player = GameObject.FindGameObjectWithTag(tag);
                if (player != null)
                {
                    totalPlayers++;
                    Debug.Log($"Jugador encontrado con tag: {tag}");
                }
            }
            catch
            {
                // Tag no existe en el proyecto, continuar
            }
        }

        Debug.Log($"⚠️ TOTAL JUGADORES DETECTADOS: {totalPlayers}");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Buscar el jugador que ha cruzado la meta
        PlayerFinishUI playerUI = players.FirstOrDefault(p => other.CompareTag(p.playerTag));

        if (playerUI == null || finishedPlayers.Contains(playerUI.playerTag))
            return; // Jugador no encontrado o ya ha terminado

        // Verificar checkpoints
        if (playerUI.checkpointCounter != null)
        {
            if (playerUI.checkpointCounter.GetCheckpointCount() < 3)
                return; // No ha pasado por todos los checkpoints
        }

        // Marcar jugador como finalizado
        finishedPlayers.Add(playerUI.playerTag);

        // Activar partículas de bengalas(busca las particulas del inspector atachadas)
        ActivateFinishParticles();

        // Procesar llegada a meta
        ProcessPlayerFinish(playerUI, other);

        // Verificar si todos los jugadores han terminado
        if (finishedPlayers.Count >= totalPlayers)
        {
            OnAllPlayersFinished();
        }
    }

    private void ProcessPlayerFinish(PlayerFinishUI playerUI, Collider playerCollider)
    {
        // 1️⃣ Obtener el tiempo actual (sin detener aún el cronómetro)
        float elapsedTime = playerUI.stopwatchTimer != null ? playerUI.stopwatchTimer.GetElapsedTime() : 0f;

        // 2️⃣ Detener el coche
        PrometeoCarController carController = playerCollider.GetComponent<PrometeoCarController>();
        if (carController != null)
        {
            carController.carSpeed = 0f;
            carController.enabled = false;
        }

        // 3️⃣ Grabar la carrera (si aplica)
        if (playerUI.carRecorder != null && playerUI.stopwatchTimer != null)
            playerUI.carRecorder.StopRecording(elapsedTime);

        // 4️⃣ Mostrar mensaje con posición
        if (playerUI.finishMessageUI != null)
        {
            Debug.Log($"🎯 Mostrando posición para {playerUI.playerTag}. Total Players: {totalPlayers}, Posición: {finishPosition}");
            
            // Para un solo jugador, mostrar "Finish!"
            // Para multijugador, mostrar la posición
            if (totalPlayers == 1)
            {
                playerUI.finishMessageUI.text = "Finish!";
                Debug.Log("Modo 1 jugador: mostrando 'Finish!'");
            }
            else
            {
                string positionText = GetPositionText(finishPosition);
                Color positionColor = GetPositionColor(finishPosition);
                playerUI.finishMessageUI.text = positionText;
                playerUI.finishMessageUI.color = positionColor;
                Debug.Log($"Modo multijugador: mostrando '{positionText}'");
            }
            playerUI.finishMessageUI.gameObject.SetActive(true);
        }

        // 5️⃣ Fade del panel
        if (playerUI.finishPanel != null)
        {
            playerUI.finishPanel.gameObject.SetActive(true);
            StartCoroutine(FadeInPanel(playerUI.finishPanel, fadeDuration, targetAlpha));
        }

        // 6️⃣ Guardar mejor tiempo (solo para Player principal)
        if (playerUI.playerTag == "Player")
        {
            BestTimeManager.SaveBestTime(elapsedTime);
        }

        // 7️⃣ Mostrar tiempo del jugador
        DisplayPlayerTime(playerUI, elapsedTime);

        // 8️⃣ Mostrar mejor tiempo (para TODOS los jugadores)
        DisplayBestTime(playerUI, elapsedTime);

        finishPosition++; // Incrementar para el siguiente jugador

        Debug.Log($"{playerUI.playerTag} ha llegado a la meta en posición {finishPosition - 1}");
    }

    private void DisplayPlayerTime(PlayerFinishUI playerUI, float elapsedTime)
    {
        if (playerUI.textYourTime == null) return;

        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int hundredths = Mathf.FloorToInt((elapsedTime * 100) % 100);

        playerUI.textYourTime.text = string.Format("Your time: {0:00}:{1:00}:{2:00}", minutes, seconds, hundredths);
        playerUI.textYourTime.gameObject.SetActive(true);
    }

    private void DisplayBestTime(PlayerFinishUI playerUI, float currentTime)
    {
        if (playerUI.textBestTime == null) return;

        float bestTime = BestTimeManager.GetBestTime();

        if (bestTime != float.MaxValue)
        {
            int minutes = Mathf.FloorToInt(bestTime / 60f);
            int seconds = Mathf.FloorToInt(bestTime % 60f);
            int hundredths = Mathf.FloorToInt((bestTime * 100) % 100);

            playerUI.textBestTime.text = string.Format("Best time: {0:00}:{1:00}:{2:00}", minutes, seconds, hundredths);
            playerUI.textBestTime.gameObject.SetActive(true);

            // Parpadear si es nuevo récord (solo para Player principal)
            if (playerUI.playerTag == "Player" && currentTime <= bestTime)
                StartCoroutine(BlinkCoroutine(playerUI.textBestTime));
        }
        else
        {
            playerUI.textBestTime.text = "Best time: --:--:--";
            playerUI.textBestTime.gameObject.SetActive(true);
        }
    }

    private string GetPositionText(int position)
    {
        switch (position)
        {
            case 1: return "1st Place!";
            case 2: return "2nd Place";
            case 3: return "3rd Place";
            case 4: return "4th Place";
            default: return $"{position}th Place";
        }
    }

    private Color GetPositionColor(int position)
    {
        switch (position)
        {
            case 1: return firstPlaceColor;
            case 2: return secondPlaceColor;
            case 3: return thirdPlaceColor;
            case 4: return fourthPlaceColor;
            default: return Color.white;
        }
    }

    private void OnAllPlayersFinished()
    {
        Debug.Log("¡Todos los jugadores han terminado!");

        // Deshabilitar pausa cuando el último jugador termina
        if (panelPauseScript != null)
        {
            panelPauseScript.canPause = false;
        }

        // Detener TODOS los cronómetros cuando el último jugador termina
        foreach (var player in players)
        {
            if (player.stopwatchTimer != null)
                player.stopwatchTimer.StopTimer();
        }

        // Fade out de la música
        if (SoundManager.Instance != null)
            StartCoroutine(SoundManager.Instance.FadeOutMusic(fadeDuration));

        // Mostrar panel de pausa después del delay
        if (panelPauseScript != null)
        {
            StartCoroutine(ActivatePausePanelAfterDelay());
        }
    }

    private IEnumerator FadeInPanel(Image panel, float duration, float targetAlpha)
    {
        float elapsed = 0f;
        Color color = panel.color;
        float startAlpha = color.a;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            color.a = alpha;
            panel.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        panel.color = color;
    }

    private IEnumerator BlinkCoroutine(TextMeshProUGUI text)
    {
        Color originalColor = text.color;
        float elapsed = 0f;

        while (elapsed < blinkDuration)
        {
            text.color = text.color == originalColor ? blinkColor : originalColor;
            elapsed += blinkSpeed;
            yield return new WaitForSeconds(blinkSpeed);
        }

        text.color = originalColor;
    }

    private IEnumerator ActivatePausePanelAfterDelay()
    {
        yield return new WaitForSeconds(delayPanelPause);

        if (panelPauseScript != null)
        {
            panelPauseScript.HidePlayButton();
            panelPauseScript.PauseGame();
        }
    }

    private void ActivateFinishParticles()
    {
        if (finishParticles != null && finishParticles.Length > 0)
        {
            // Activar todas las partículas asignadas (bengalas, cohetes, etc.)
            foreach (var ps in finishParticles)
            {
                if (ps != null)
                {
                    ps.Play();
                }
            }
        }
    }
}