using UnityEngine;

/// <summary>
/// AtlasColorMask
/// - Mantiene una copia runtime del atlas (Texture2D) y permite pintar regiones usando máscaras
///   o buscando un color clave en una imagen de referencia (útil para detectar tu púrpura A349A4).
/// - Soporta modos de mezcla básicos: replace, multiply y preserveLuminance (recomendada).
/// - Al final aplica la textura resultante a los materiales del `Renderer` proporcionado.
/// </summary>
public class AtlasColorMask : MonoBehaviour
{
    [Tooltip("Atlas fuente (asignar el atlas importado). Si no es readable se intentará crear una copia via RenderTexture.")]
    public Texture2D atlasTexture;

    [Tooltip("Máscara del color púrpura (opcional). Si se asigna, se usará esta máscara en lugar de detectar por color cada vez.")]
    public Texture2D purpleMask;

    // copia en memoria que modificaremos en runtime
    private Texture2D runtimeAtlas;

    // umbral para considerar un píxel de máscara como 'activo' (0..1)
    public float maskThreshold = 0.5f;

    void Awake()
    {
        if (atlasTexture != null)
            CreateRuntimeCopy();
    }

    // Asegura que runtimeAtlas existe (crea copia si hace falta)
    private void EnsureRuntimeAtlas()
    {
        if (runtimeAtlas != null) return;
        if (atlasTexture == null)
        {
            Debug.LogWarning("AtlasColorMask: no hay `atlasTexture` asignada.");
            return;
        }
        CreateRuntimeCopy();
    }

    // Intenta crear una copia modificable del atlas original.
    private void CreateRuntimeCopy()
    {
        if (atlasTexture == null) return;

        int w = atlasTexture.width;
        int h = atlasTexture.height;

        // Intento directo (GetPixels) — funciona si la textura es Read/Write enabled
        try
        {
            Color[] pixels = atlasTexture.GetPixels();
            runtimeAtlas = new Texture2D(w, h, TextureFormat.RGBA32, false);
            runtimeAtlas.SetPixels(pixels);
            runtimeAtlas.Apply();
            // name the runtime copy so logs and saved files are identifiable
            try { runtimeAtlas.name = atlasTexture.name + "_runtime"; } catch {}
            return;
        }
        catch (System.Exception)
        {
            // Caeremos a la copia via RenderTexture si no es readable.
        }

        // Fallback: blit a RenderTexture y ReadPixels (funciona aunque la fuente no sea readable)
        RenderTexture rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        RenderTexture previous = RenderTexture.active;
        try
        {
            Graphics.Blit(atlasTexture, rt);
            RenderTexture.active = rt;

            runtimeAtlas = new Texture2D(w, h, TextureFormat.RGBA32, false);
            runtimeAtlas.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            runtimeAtlas.Apply();
            try { runtimeAtlas.name = atlasTexture.name + "_runtime"; } catch {}
        }
        finally
        {
            // Restaurar el RenderTexture activo antes de liberar
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
        }
    }

    /// <summary>
    /// Public helper: pinta las regiones con el color elegido y aplica inmediatamente
    /// al renderer. Si hay una purpleMask asignada, la usa. Si no, detecta por color clave.
    /// Útil para llamadas repetidas desde UI sin responsabilidad del llamador.
    /// </summary>
    public void PaintAndApply(Color keyColor, float tolerance, Color paintColor, Renderer carRenderer, string blendMode = "preserveLuminance")
    {
        if (purpleMask != null)
        {
            // Modo rápido: usar la máscara precalculada
            PaintRegionByMask(purpleMask, paintColor, blendMode);
        }
        else
        {
            // Modo detección: buscar el color clave cada vez (más lento)
            PaintRegionsMatchingColor(this.atlasTexture, keyColor, tolerance, paintColor, blendMode);
        }
        ApplyToRenderer(carRenderer);
    }

    /// <summary>
    /// Restablece la copia runtime desde la textura original (descarta cambios acumulados).
    /// </summary>
    public void ResetRuntimeAtlas()
    {
        if (atlasTexture == null) return;
        // Destrozar la copia anterior y forzar recreación
        if (runtimeAtlas != null)
        {
            try { Destroy(runtimeAtlas); } catch {}
            runtimeAtlas = null;
        }
        CreateRuntimeCopy();
    }

    /// <summary>
    /// Pinta una región definida por una máscara (blanco/alpha = área) con el color dado.
    /// Mask puede tener otra resolución; se muestrea bilinealmente para mapear al atlas.
    /// </summary>
    public void PaintRegionByMask(Texture2D mask, Color paintColor, string blendMode = "preserveLuminance")
    {
        if (mask == null)
        {
            Debug.LogWarning("AtlasColorMask.PaintRegionByMask: mask es null.");
            return;
        }

        EnsureRuntimeAtlas();
        if (runtimeAtlas == null) return;

        int w = runtimeAtlas.width;
        int h = runtimeAtlas.height;

        Color[] src = runtimeAtlas.GetPixels();
        Color[] dst = new Color[src.Length];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int idx = y * w + x;
                float u = (x + 0.5f) / (float)w;
                float v = (y + 0.5f) / (float)h;

                Color maskC = mask.GetPixelBilinear(u, v);
                float maskVal = maskC.a; // asume máscara en canal alfa
                if (maskVal >= maskThreshold || maskC.grayscale >= maskThreshold)
                {
                    dst[idx] = BlendPixel(src[idx], paintColor, blendMode);
                }
                else
                {
                    dst[idx] = src[idx];
                }
            }
        }

        runtimeAtlas.SetPixels(dst);
        runtimeAtlas.Apply();
    }

    /// <summary>
    /// Busca píxeles en una imagen fuente que coincidan con `keyColor` dentro de `tolerance` (0..1)
    /// y pinta esas zonas en el atlas con `paintColor`. Útil para detectar tu púrpura A349A4.
    /// </summary>
    public void PaintRegionsMatchingColor(Texture2D sourceReference, Color keyColor, float tolerance, Color paintColor, string blendMode = "preserveLuminance")
    {
        if (sourceReference == null)
        {
            Debug.LogWarning("AtlasColorMask.PaintRegionsMatchingColor: sourceReference es null.");
            return;
        }

        EnsureRuntimeAtlas();
        if (runtimeAtlas == null) return;

        int w = runtimeAtlas.width;
        int h = runtimeAtlas.height;
        Color[] src = runtimeAtlas.GetPixels();
        Color[] dst = new Color[src.Length];

        // Decide source for sampling: prefer readable sourceReference; else use runtimeAtlas; else create a temporary readable copy
        Texture2D samplingSource = sourceReference;
        Texture2D tempCopy = null;
        bool usingTemp = false;
        if (samplingSource != null)
        {
            bool readable = true;
            try { readable = samplingSource.isReadable; } catch { readable = false; }
            if (!readable)
            {
                if (runtimeAtlas != null)
                {
                    samplingSource = runtimeAtlas;
                }
                else
                {
                    tempCopy = CreateReadableCopy(sourceReference);
                    if (tempCopy != null)
                    {
                        samplingSource = tempCopy;
                        usingTemp = true;
                    }
                }
            }
        }

        int matchedCount = 0;
        float minDist = float.MaxValue;
        float maxDist = 0f;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int idx = y * w + x;
                float u = (x + 0.5f) / (float)w;
                float v = (y + 0.5f) / (float)h;

                Color refC = Color.clear;
                if (samplingSource != null)
                {
                    try
                    {
                        refC = samplingSource.GetPixelBilinear(u, v);
                    }
                    catch (System.Exception)
                    {
                        // fallback to nearest pixel
                        int sx = Mathf.Clamp((int)(u * samplingSource.width), 0, samplingSource.width - 1);
                        int sy = Mathf.Clamp((int)(v * samplingSource.height), 0, samplingSource.height - 1);
                        refC = samplingSource.GetPixel(sx, sy);
                    }
                }

                float dist = ColorDistance(refC, keyColor);
                if (dist < minDist) minDist = dist;
                if (dist > maxDist) maxDist = dist;
                if (dist <= tolerance)
                {
                    dst[idx] = BlendPixel(src[idx], paintColor, blendMode);
                    matchedCount++;
                }
                else
                {
                    dst[idx] = src[idx];
                }
            }
        }

        runtimeAtlas.SetPixels(dst);
        runtimeAtlas.Apply();

        if (debugLogs)
        {
            Debug.Log($"AtlasColorMask: PaintRegionsMatchingColor finished. keyColor={FormatColor(keyColor)}, tolerance={tolerance}, matched={matchedCount}, minDist={minDist:F4}, maxDist={maxDist:F4}");
        }

        if (usingTemp && tempCopy != null)
        {
            // destroy temporary copy
            try { UnityEngine.Object.Destroy(tempCopy); } catch {}
        }
    }

    /// <summary>
    /// Sobrecarga conveniente: usa el `atlasTexture` asignado como referencia.
    /// </summary>
    public void PaintRegionsMatchingColor(Color keyColor, float tolerance, Color paintColor, string blendMode = "preserveLuminance")
    {
        PaintRegionsMatchingColor(this.atlasTexture, keyColor, tolerance, paintColor, blendMode);
    }

    // Aplica runtimeAtlas a los materials del renderer. Instancia materiales para no alterar los compartidos.
    public bool debugLogs = false;
    public bool usePropertyBlock = true;
    [Tooltip("Si está activado, después de asignar la textura, resetea las propiedades de color (_Color / _BaseColor) a blanco para evitar tintados del shader.")]
    public bool resetMaterialColor = true;
    [Tooltip("Si está activado y debugLogs=true, guarda automáticamente el atlas runtime en Application.persistentDataPath después de aplicarlo (útil para verificar si el pintado funcionó).")]
    public bool debugSaveAfterApply = false;

    // Aplica runtimeAtlas a los materials del renderer. Instancia materiales para no alterar los compartidos.
    public void ApplyToRenderer(Renderer r)
    {
        if (r == null)
        {
            Debug.LogWarning("AtlasColorMask.ApplyToRenderer: Renderer es null.");
            return;
        }
        EnsureRuntimeAtlas();
        if (runtimeAtlas == null) return;
        string[] candidates = new string[] { "_MainTex", "_BaseMap", "_BaseColorMap", "_BaseTexture", "_AlbedoMap" };

        Material[] mats = r.materials; // obtiene instancias si es necesario
        bool anySet = false;
        for (int i = 0; i < mats.Length; i++)
        {
            var mat = mats[i];
            if (mat == null) continue;

            bool set = false;
            string usedProp = null;
            foreach (var prop in candidates)
            {
                if (mat.HasProperty(prop))
                {
                    mat.SetTexture(prop, runtimeAtlas);
                    usedProp = prop;
                    set = true;
                    anySet = true;
                    break;
                }
            }

            if (!set)
            {
                // fallback directo
                try
                {
                    mat.mainTexture = runtimeAtlas;
                    usedProp = "mainTexture";
                    set = true;
                    anySet = true;
                }
                catch
                {
                    // ignore
                }
            }

            if (debugLogs)
            {
                Debug.Log($"AtlasColorMask: applied atlas to material '{mat.name}' using '{(usedProp ?? "(none)")}'");
            }
            // Opcional: resetear color del material para prevenir tintado
            if (resetMaterialColor)
            {
                try { if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white); } catch {}
                try { if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white); } catch {}
            }
        }

        // Re-assign materials array to ensure renderer uses updated instances
        try
        {
            r.materials = mats;
        }
        catch {}

        // Special-case: algunos materiales del proyecto usan nombres concretos
        for (int i = 0; i < mats.Length; i++)
        {
            var mat = mats[i];
            if (mat == null) continue;
            string lname = mat.name.ToLowerInvariant();
            if (lname.Contains("pcc_col_mat") || lname.Contains("pcc_light_mat"))
            {
                try
                {
                    mat.SetTexture("_MainTex", runtimeAtlas);
                }
                catch {}
                try
                {
                    mat.SetTexture("_BaseMap", runtimeAtlas);
                }
                catch {}
                try
                {
                    mat.mainTexture = runtimeAtlas;
                }
                catch {}
                anySet = true;
                if (debugLogs) Debug.Log($"AtlasColorMask: forced atlas assign for special material '{mat.name}'");
                if (resetMaterialColor)
                {
                    try { if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white); } catch {}
                    try { if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white); } catch {}
                    try { if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", Color.black); } catch {}
                }
            }
        }

        // Also try using MaterialPropertyBlock per-renderer as a non-instancing alternative
        if (usePropertyBlock && runtimeAtlas != null)
        {
            var mpb = new MaterialPropertyBlock();
            // try main candidates
            foreach (var prop in new string[] { "_MainTex", "_BaseMap" })
            {
                try
                {
                    mpb.SetTexture(prop, runtimeAtlas);
                }
                catch {}
            }
            // ensure property block also resets shader tint colors
            try { mpb.SetColor("_BaseColor", Color.white); } catch {}
            try { mpb.SetColor("_Color", Color.white); } catch {}
            r.SetPropertyBlock(mpb);
            if (debugLogs) Debug.Log($"AtlasColorMask: set MaterialPropertyBlock on renderer '{r.name}'");
            anySet = true;
        }

        if (!anySet && debugLogs)
        {
            Debug.LogWarning($"AtlasColorMask: no se pudo aplicar la textura al renderer '{r.name}' — revisa propiedades del shader/material.");
        }

        // Diagnostic: listar info de materiales y comprobar que la textura asignada es la esperada
        if (debugLogs)
        {
            try
            {
                for (int i = 0; i < mats.Length; i++)
                {
                    var mat = mats[i];
                    if (mat == null) continue;
                    Debug.Log($"AtlasColorMask: material[{i}] name='{mat.name}', shader='{mat.shader.name}'");
                    string[] diagProps = new string[] { "_MainTex", "_BaseMap", "_BaseColorMap", "_BaseTexture", "_AlbedoMap" };
                    foreach (var p in diagProps)
                    {
                        if (mat.HasProperty(p))
                        {
                            var t = mat.GetTexture(p);
                            Debug.Log($"    prop '{p}' present -> texture: " + (t == null ? "(null)" : t.name + (t == runtimeAtlas ? " (runtimeAtlas)" : "")));
                        }
                    }
                    // also log mainTexture
                    try { var mt = mat.mainTexture; Debug.Log($"    mainTexture: " + (mt == null ? "(null)" : mt.name + (mt == runtimeAtlas ? " (runtimeAtlas)" : ""))); } catch {}
                }
                // Also log property block textures if present (difficult to inspect, but we saved the atlas file optionally)
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("AtlasColorMask: diagnostic logging failed: " + ex.Message);
            }
        }

        // Guardar el atlas runtime automático si se pidió (útil para inspeccionar si el pintado cambió realmente los píxeles)
        if (debugSaveAfterApply && debugLogs)
        {
            try
            {
                SaveRuntimeAtlas();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("AtlasColorMask: SaveRuntimeAtlas failed: " + ex.Message);
            }
        }
    }

    /// <summary>
    /// Intenta forzar la asignación del atlas a todas las propiedades de textura del shader
    /// (inspecciona las propiedades del shader si la API está disponible) y usa MaterialPropertyBlock.
    /// Útil cuando los nombres de las propiedades son personalizados.
    /// </summary>
    public void ForceApplyToRenderer(Renderer r)
    {
        if (r == null) return;
        EnsureRuntimeAtlas();
        if (runtimeAtlas == null) return;

        // Broader list of candidate texture property names to try
        string[] candidates = new string[] { "_MainTex", "_BaseMap", "_BaseColorMap", "_BaseTexture", "_AlbedoMap", "_DetailAlbedoMap", "_EmissionMap", "_MetallicGlossMap", "_SpecGlossMap", "_BumpMap", "_ParallaxMap" };

        Material[] mats = r.materials; // instancias
        bool any = false;

        for (int i = 0; i < mats.Length; i++)
        {
            var mat = mats[i];
            if (mat == null) continue;

            foreach (var prop in candidates)
            {
                if (mat.HasProperty(prop))
                {
                    try { mat.SetTexture(prop, runtimeAtlas); any = true; } catch {}
                }
            }

            // fallback directo
            try { mat.mainTexture = runtimeAtlas; any = true; } catch {}
        }

        // reasignar materiales y property block
        try { r.materials = mats; } catch {}
        if (usePropertyBlock)
        {
            var mpb = new MaterialPropertyBlock();
            try { mpb.SetTexture("_MainTex", runtimeAtlas); } catch {}
            try { r.SetPropertyBlock(mpb); } catch {}
        }

        if (debugLogs) Debug.Log($"AtlasColorMask.ForceApplyToRenderer: applied textures to renderer '{r.name}' (any={any})");
    }

    /// <summary>
    /// Imprime información útil del renderer y sus materiales (shader y propiedades de textura).
    /// </summary>
    public void DumpMaterialInfo(Renderer r)
    {
        if (r == null) return;
        Material[] mats = r.sharedMaterials;
        Debug.Log($"AtlasColorMask.DumpMaterialInfo for renderer '{r.name}' - materials count: {mats?.Length ?? 0}");
        if (mats == null) return;
        for (int i = 0; i < mats.Length; i++)
        {
            var mat = mats[i];
            if (mat == null) { Debug.Log($"  [{i}] null"); continue; }
            Debug.Log($"  [{i}] name='{mat.name}', shader='{mat.shader.name}'");
            // Listado simple de propiedades de textura comunes que el material expone
            string[] candidates = new string[] { "_MainTex", "_BaseMap", "_BaseColorMap", "_BaseTexture", "_AlbedoMap", "_DetailAlbedoMap", "_EmissionMap", "_MetallicGlossMap", "_SpecGlossMap", "_BumpMap", "_ParallaxMap" };
            foreach (var prop in candidates)
            {
                if (mat.HasProperty(prop)) Debug.Log($"    - tex prop: {prop}");
            }
        }
    }

    /// <summary>
    /// Método de prueba invocable desde el Inspector (Context Menu).
    /// Ejecuta DumpMaterialInfo, ForceApplyToRenderer y ApplyToRenderer para diagnosticar/aplicar.
    /// </summary>
    [ContextMenu("Test Apply Atlas")]
    public void TestApply()
    {
        // intenta encontrar un renderer en el mismo GameObject o en hijos si no se proporciona
        Renderer r = GetComponent<Renderer>();
        if (r == null) r = GetComponentInChildren<Renderer>();
        if (r == null)
        {
            Debug.LogWarning("AtlasColorMask.TestApply: no se encontró Renderer en el mismo GameObject ni en hijos.");
            return;
        }

        Debug.Log($"AtlasColorMask.TestApply: using renderer '{r.name}'");
        DumpMaterialInfo(r);
        ForceApplyToRenderer(r);
        ApplyToRenderer(r);
        Debug.Log("AtlasColorMask.TestApply: finished");
    }

    [ContextMenu("Paint Test Stripe")]
    public void PaintTestStripe()
    {
        EnsureRuntimeAtlas();
        if (runtimeAtlas == null)
        {
            Debug.LogWarning("AtlasColorMask.PaintTestStripe: no runtimeAtlas disponible.");
            return;
        }

        int w = runtimeAtlas.width;
        int h = runtimeAtlas.height;
        Color[] pix = runtimeAtlas.GetPixels();
        int stripeX = Mathf.Clamp(w / 8, 1, w - 2);
        int stripeWidth = Mathf.Clamp(w / 40, 2, 10);
        Color stripeColor = Color.red;

        for (int y = 0; y < h; y++)
        {
            for (int sx = 0; sx < stripeWidth; sx++)
            {
                int x = stripeX + sx;
                if (x < 0 || x >= w) continue;
                int idx = y * w + x;
                pix[idx] = stripeColor;
            }
        }

        runtimeAtlas.SetPixels(pix);
        runtimeAtlas.Apply();
        Debug.Log("AtlasColorMask.PaintTestStripe: painted stripe and applied runtimeAtlas.");
        SaveRuntimeAtlas();

        // apply to renderer on same GameObject or children
        Renderer r = GetComponent<Renderer>();
        if (r == null) r = GetComponentInChildren<Renderer>();
        if (r != null) ApplyToRenderer(r);
    }

    private string FormatColor(Color c)
    {
        return $"({c.r:F3},{c.g:F3},{c.b:F3},{c.a:F3})";
    }

    [ContextMenu("Save Assigned Textures")]
    public void SaveAssignedTextures()
    {
        Renderer r = GetComponent<Renderer>();
        if (r == null) r = GetComponentInChildren<Renderer>();
        if (r == null)
        {
            Debug.LogWarning("AtlasColorMask.SaveAssignedTextures: no Renderer found.");
            return;
        }
        Material[] mats = r.sharedMaterials;
        if (mats == null) return;
        foreach (var mat in mats)
        {
            if (mat == null) continue;
            string matName = mat.name.Replace(' ', '_');
            string[] props = new string[] { "_MainTex", "_BaseMap", "_BaseColorMap", "_BaseTexture", "_AlbedoMap" };
            foreach (var p in props)
            {
                if (!mat.HasProperty(p)) continue;
                Texture t = mat.GetTexture(p);
                if (t == null) continue;
                Texture2D copy = null;
                if (t is Texture2D tt)
                {
                    copy = CreateReadableCopy(tt);
                }
                else if (t is RenderTexture rt)
                {
                    RenderTexture prev = RenderTexture.active;
                    try
                    {
                        RenderTexture.active = rt;
                        copy = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
                        copy.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                        copy.Apply();
                    }
                    finally
                    {
                        RenderTexture.active = prev;
                    }
                }
                if (copy != null)
                {
                    try
                    {
                        string path = System.IO.Path.Combine(Application.persistentDataPath, $"assigned_{matName}_{p}.png");
                        System.IO.File.WriteAllBytes(path, copy.EncodeToPNG());
                        Debug.Log($"AtlasColorMask: saved assigned texture {p} of '{mat.name}' to {path}");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning("AtlasColorMask: failed to save assigned texture: " + ex.Message);
                    }
                    finally
                    {
                        try { Destroy(copy); } catch {};
                    }
                }
                else
                {
                    Debug.LogWarning($"AtlasColorMask: could not create readable copy of texture property {p} on material {mat.name}");
                }
            }
        }
    }

    [ContextMenu("Compare Assigned To RuntimeAtlas")]
    public void CompareAssignedTextureToRuntimeAtlas()
    {
        EnsureRuntimeAtlas();
        if (runtimeAtlas == null)
        {
            Debug.LogWarning("AtlasColorMask.CompareAssignedTextureToRuntimeAtlas: no runtimeAtlas.");
            return;
        }
        Renderer r = GetComponent<Renderer>();
        if (r == null) r = GetComponentInChildren<Renderer>();
        if (r == null)
        {
            Debug.LogWarning("AtlasColorMask.CompareAssignedTextureToRuntimeAtlas: no Renderer found.");
            return;
        }
        Material[] mats = r.sharedMaterials;
        if (mats == null) return;
        foreach (var mat in mats)
        {
            if (mat == null) continue;
            string[] props = new string[] { "_MainTex", "_BaseMap", "_BaseColorMap", "_BaseTexture", "_AlbedoMap" };
            foreach (var p in props)
            {
                if (!mat.HasProperty(p)) continue;
                Texture t = mat.GetTexture(p);
                if (t == null) continue;
                if (t == runtimeAtlas)
                {
                    Debug.Log($"AtlasColorMask: material '{mat.name}' property {p} references the same runtimeAtlas instance.");
                    continue;
                }
                Texture2D other = null;
                if (t is Texture2D tt) other = CreateReadableCopy(tt);
                else if (t is RenderTexture rt)
                {
                    RenderTexture prev = RenderTexture.active;
                    try { RenderTexture.active = rt; other = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false); other.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0); other.Apply(); }
                    finally { RenderTexture.active = prev; }
                }
                if (other == null)
                {
                    Debug.LogWarning($"AtlasColorMask: could not read texture {p} on material {mat.name} for comparison.");
                    continue;
                }
                // compare sizes first
                if (other.width != runtimeAtlas.width || other.height != runtimeAtlas.height)
                {
                    Debug.Log($"AtlasColorMask: assigned texture {p} on material {mat.name} has different size ({other.width}x{other.height}) vs runtimeAtlas ({runtimeAtlas.width}x{runtimeAtlas.height}).");
                    Destroy(other);
                    continue;
                }
                Color[] a = runtimeAtlas.GetPixels();
                Color[] b = other.GetPixels();
                int diff = 0;
                for (int i = 0; i < a.Length; i++) if (ColorDistance(a[i], b[i]) > 0.01f) diff++;
                Debug.Log($"AtlasColorMask: comparison for material '{mat.name}' prop {p}: differing pixels = {diff} / {a.Length}");
                try { Destroy(other); } catch {};
            }
        }
    }

    /// <summary>
    /// Guarda la `runtimeAtlas` actual en `Application.persistentDataPath` para inspección.
    /// </summary>
    [ContextMenu("Save Runtime Atlas PNG")]
    public void SaveRuntimeAtlas()
    {
        EnsureRuntimeAtlas();
        if (runtimeAtlas == null)
        {
            Debug.LogWarning("AtlasColorMask.SaveRuntimeAtlas: no hay runtimeAtlas para guardar.");
            return;
        }
        try
        {
            byte[] bytes = runtimeAtlas.EncodeToPNG();
            string path = System.IO.Path.Combine(Application.persistentDataPath, runtimeAtlas.name + "_runtime.png");
            System.IO.File.WriteAllBytes(path, bytes);
            Debug.Log($"AtlasColorMask: saved runtime atlas to {path}");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("AtlasColorMask.SaveRuntimeAtlas: failed to save: " + ex.Message);
        }
    }

    /// <summary>
    /// Aplicar directamente el color seleccionado a propiedades de color del material (shader tint).
    /// Útil para comprobar si el shader está usando esa propiedad para recolorizar.
    /// </summary>
    public void ApplyColorToMaterialProperties(Renderer r, Color color)
    {
        if (r == null) return;
        Material[] mats = r.materials;
        for (int i = 0; i < mats.Length; i++)
        {
            var mat = mats[i];
            if (mat == null) continue;
            try { if (mat.HasProperty("_Color")) mat.SetColor("_Color", color); } catch {}
            try { if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color); } catch {}
            try { if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", color); } catch {}
        }
        try { r.materials = mats; } catch {}
    }

    /// <summary>
    /// Blend básico entre píxel original y color seleccionado.
    /// - "replace": devuelve paintColor (alpha del original preservada)
    /// - "multiply": original.rgb * paint.rgb
    /// - "preserveLuminance": uso HSV: hue/sat del paint, value (luminancia) del original
    /// </summary>
    private Color BlendPixel(Color original, Color paint, string mode)
    {
        mode = (mode ?? "").ToLowerInvariant();
        if (mode == "replace")
        {
            return new Color(paint.r, paint.g, paint.b, original.a);
        }
        else if (mode == "multiply")
        {
            return new Color(original.r * paint.r, original.g * paint.g, original.b * paint.b, original.a);
        }
        else // preserveLuminance por defecto
        {
            Color.RGBToHSV(original, out float h0, out float s0, out float v0);
            Color.RGBToHSV(paint, out float hp, out float sp, out float vp);
            Color result = Color.HSVToRGB(hp, sp, v0);
            result.a = original.a;
            return result;
        }
    }

    // Distancia simple en RGB (euclidiana) normalizada 0..1
    private float ColorDistance(Color a, Color b)
    {
        float dr = a.r - b.r;
        float dg = a.g - b.g;
        float db = a.b - b.b;
        return Mathf.Sqrt(dr * dr + dg * dg + db * db);
    }

    // Método auxiliar simple que implementa el API anterior minimal: repintar todo el coche con un color (tint)
    public void PaintCar(Color color, Renderer carRenderer)
    {
        if (carRenderer == null)
            return;

        EnsureRuntimeAtlas();
        if (runtimeAtlas == null)
        {
            // fallback: simplemente cambiar color en materiales
            Material[] mats = carRenderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;
                if (mats[i].HasProperty("_Color")) mats[i].SetColor("_Color", color);
                else mats[i].color = color;
            }
            carRenderer.materials = mats;
            return;
        }

        // Aplicar runtimeAtlas (sin cambiar pixeles) — útil si ya hemos pintado antes
        ApplyToRenderer(carRenderer);
    }

    // Crea una copia readable (Texture2D) a partir de cualquier Texture2D fuente (incluso si no es readable)
    // Devuelve la nueva Texture2D (debe ser destruida por el llamador cuando ya no se necesite).
    private Texture2D CreateReadableCopy(Texture2D src)
    {
        if (src == null) return null;
        int w = src.width;
        int h = src.height;
        RenderTexture rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        RenderTexture previous = RenderTexture.active;
        try
        {
            Graphics.Blit(src, rt);
            RenderTexture.active = rt;
            Texture2D copy = new Texture2D(w, h, TextureFormat.RGBA32, false);
            copy.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            copy.Apply();
            return copy;
        }
        catch
        {
            return null;
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
        }
    }
}

