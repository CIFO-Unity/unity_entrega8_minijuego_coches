using UnityEngine;

public static class BestTimeManager
{
    private const string BestTimeKey = "BestTime";

    // Guarda el tiempo si es mejor (menor que el actual guardado)
    public static void SaveBestTime(float newTime)
    {
        float currentBest = PlayerPrefs.GetFloat(BestTimeKey, float.MaxValue);

        if (newTime < currentBest)
        {
            PlayerPrefs.SetFloat(BestTimeKey, newTime);
            PlayerPrefs.Save();
            Debug.Log("Nuevo mejor tiempo guardado: " + newTime);
        }
    }

    // Obtiene el mejor tiempo guardado (si no hay, devuelve float.MaxValue)
    public static float GetBestTime()
    {
        return PlayerPrefs.GetFloat(BestTimeKey, float.MaxValue);
    }
}
