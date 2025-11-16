using UnityEngine;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// Componente para botón que guarda el atlas modificado por el usuario.
/// Guarda el runtimeAtlas actual con el color aplicado en Assets/Textures/Masks/AtlasUsuario.png
/// </summary>
public class SaveAtlasButton : MonoBehaviour
{
    [Tooltip("Referencia al AtlasColorMask que contiene el atlas modificado")]
    public AtlasColorMask atlasColorMask;

    [Tooltip("Nombre del archivo a guardar (sin extensión)")]
    public string fileName = "AtlasUsuario";

    [Tooltip("Carpeta de destino relativa a Assets")]
    public string targetFolder = "Textures/Masks";

    [Tooltip("Mostrar logs en consola")]
    public bool showLogs = true;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(SaveAtlas);
        }
    }

    /// <summary>
    /// Guarda el atlas modificado en la carpeta especificada
    /// </summary>
    public void SaveAtlas()
    {
        if (atlasColorMask == null)
        {
            Debug.LogError("SaveAtlasButton: No se ha asignado AtlasColorMask en el Inspector.");
            return;
        }

        // Acceder al runtimeAtlas mediante reflexión (es privado)
        var runtimeAtlasField = typeof(AtlasColorMask).GetField("runtimeAtlas", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (runtimeAtlasField == null)
        {
            Debug.LogError("SaveAtlasButton: No se pudo acceder al campo runtimeAtlas.");
            return;
        }

        Texture2D runtimeAtlas = runtimeAtlasField.GetValue(atlasColorMask) as Texture2D;

        if (runtimeAtlas == null)
        {
            Debug.LogError("SaveAtlasButton: El runtimeAtlas es null. Asegúrate de haber aplicado un color primero.");
            return;
        }

        // Construir ruta completa
        string projectPath = Application.dataPath; // ruta a la carpeta Assets
        string fullFolderPath = Path.Combine(projectPath, targetFolder);
        
        // Crear directorio si no existe
        if (!Directory.Exists(fullFolderPath))
        {
            Directory.CreateDirectory(fullFolderPath);
            if (showLogs)
                Debug.Log($"SaveAtlasButton: Carpeta creada en {fullFolderPath}");
        }

        string fullFilePath = Path.Combine(fullFolderPath, fileName + ".png");

        try
        {
            // Codificar a PNG y guardar
            byte[] pngBytes = runtimeAtlas.EncodeToPNG();
            File.WriteAllBytes(fullFilePath, pngBytes);

            if (showLogs)
            {
                Debug.Log($"SaveAtlasButton: Atlas guardado exitosamente en:\n{fullFilePath}");
                Debug.Log($"SaveAtlasButton: Tamaño: {runtimeAtlas.width}x{runtimeAtlas.height}");
            }

#if UNITY_EDITOR
            // Refrescar el asset database para que Unity detecte el nuevo archivo
            UnityEditor.AssetDatabase.Refresh();
            
            // Ruta relativa para el log en el Editor
            string relativePath = "Assets/" + targetFolder + "/" + fileName + ".png";
            Debug.Log($"SaveAtlasButton: Archivo visible en Unity en: {relativePath}");
#endif
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"SaveAtlasButton: Error al guardar el atlas: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Método auxiliar para llamar desde el Inspector o desde otros scripts
    /// </summary>
    [ContextMenu("Guardar Atlas Ahora")]
    public void SaveAtlasNow()
    {
        SaveAtlas();
    }
}
