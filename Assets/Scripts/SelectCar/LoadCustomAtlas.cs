using UnityEngine;
using System.IO;

/// <summary>
/// Carga el AtlasUsuario.png guardado previamente y lo aplica al coche del jugador.
/// Coloca este script en el coche del jugador en la escena Circuito.
/// </summary>
public class LoadCustomAtlas : MonoBehaviour
{
    [Tooltip("Renderer del coche al que aplicar el atlas personalizado")]
    public Renderer carRenderer;

    [Tooltip("Nombre del archivo del atlas personalizado (sin extensión)")]
    public string customAtlasFileName = "AtlasUsuario";

    [Tooltip("Carpeta donde se guardó el atlas (relativa a Assets)")]
    public string atlasFolder = "Textures/Masks";

    [Tooltip("Cargar automáticamente en Start")]
    public bool loadOnStart = true;

    [Tooltip("Mostrar logs de depuración")]
    public bool showLogs = true;

    void Start()
    {
        if (loadOnStart)
        {
            LoadCustomTexture();
        }
    }

    /// <summary>
    /// Carga el atlas personalizado desde la carpeta especificada y lo aplica al renderer
    /// </summary>
    public void LoadCustomTexture()
    {
        if (carRenderer == null)
        {
            carRenderer = GetComponent<Renderer>();
            if (carRenderer == null)
            {
                carRenderer = GetComponentInChildren<Renderer>();
            }
        }

        if (carRenderer == null)
        {
            Debug.LogError("LoadCustomAtlas: No se encontró Renderer. Asigna manualmente el carRenderer.");
            return;
        }

        // Construir ruta completa
        string fullPath = Path.Combine(Application.dataPath, atlasFolder, customAtlasFileName + ".png");

        if (!File.Exists(fullPath))
        {
            if (showLogs)
            {
                Debug.LogWarning($"LoadCustomAtlas: No se encontró el atlas personalizado en:\n{fullPath}\nUsando textura por defecto.");
            }
            return;
        }

        try
        {
            // Leer archivo PNG
            byte[] fileData = File.ReadAllBytes(fullPath);
            
            // Crear textura
            Texture2D customTexture = new Texture2D(2, 2); // Tamaño temporal, LoadImage lo ajustará
            if (customTexture.LoadImage(fileData))
            {
                customTexture.name = customAtlasFileName;
                
                // Aplicar a todos los materiales del renderer
                Material[] materials = carRenderer.materials;
                bool applied = false;

                foreach (var mat in materials)
                {
                    if (mat == null) continue;

                    // Intentar propiedades comunes de textura
                    if (mat.HasProperty("_MainTex"))
                    {
                        mat.SetTexture("_MainTex", customTexture);
                        applied = true;
                    }
                    if (mat.HasProperty("_BaseMap"))
                    {
                        mat.SetTexture("_BaseMap", customTexture);
                        applied = true;
                    }

                    // Resetear color del material a blanco para evitar tintados
                    if (mat.HasProperty("_Color"))
                        mat.SetColor("_Color", Color.white);
                    if (mat.HasProperty("_BaseColor"))
                        mat.SetColor("_BaseColor", Color.white);
                }

                // Aplicar también con MaterialPropertyBlock
                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                mpb.SetTexture("_MainTex", customTexture);
                mpb.SetTexture("_BaseMap", customTexture);
                carRenderer.SetPropertyBlock(mpb);

                if (showLogs)
                {
                    Debug.Log($"LoadCustomAtlas: Atlas personalizado cargado y aplicado exitosamente desde:\n{fullPath}\nTamaño: {customTexture.width}x{customTexture.height}");
                }
            }
            else
            {
                Debug.LogError("LoadCustomAtlas: Error al cargar la imagen PNG.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LoadCustomAtlas: Error al cargar el atlas: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Método auxiliar para llamar desde el Inspector o desde otros scripts
    /// </summary>
    [ContextMenu("Cargar Atlas Ahora")]
    public void LoadAtlasNow()
    {
        LoadCustomTexture();
    }

    /// <summary>
    /// Resetea al atlas original (útil para testing)
    /// </summary>
    [ContextMenu("Resetear a Atlas Original")]
    public void ResetToOriginalAtlas()
    {
        if (carRenderer == null) return;

        Material[] materials = carRenderer.materials;
        foreach (var mat in materials)
        {
            if (mat == null) continue;
            
            // Intentar restaurar textura original (esto solo funciona si el material tiene una referencia guardada)
            if (mat.HasProperty("_MainTex"))
            {
                // Unity no guarda referencia al original, necesitarías implementar tu propia lógica
                if (showLogs)
                    Debug.Log("LoadCustomAtlas: Para resetear necesitas guardar una referencia al atlas original.");
            }
        }
    }
}
