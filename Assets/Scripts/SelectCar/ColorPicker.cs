using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorPicker : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public Image wheelImage;
    public RectTransform selector;
    public AtlasColorMask mask;
    public Renderer carRenderer;
    public Image previewImage;
    [Tooltip("Tolerancia para la detección del color clave en el atlas (0..1). Aumenta si no detecta el púrpura exacto.")]
    public float keyColorTolerance = 0.08f;

    // cached selector Image component (optional)
    private Image selectorImage;

    private Texture2D wheelTexture;

    void Start()
    {
        try{
            if(wheelImage != null && wheelImage.sprite != null){
                wheelTexture = wheelImage.sprite.texture;
            }else{
                wheelTexture = null;
            }
        }catch(System.Exception ex){
            Debug.LogWarning("ColorPicker: could not read wheel texture: " + ex.Message);
            wheelTexture = null;
        }

        // cache selector image if available
        if(selector != null){
            selectorImage = selector.GetComponent<Image>();
            if(selectorImage == null){
                selectorImage = selector.GetComponentInChildren<Image>();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateColor(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateColor(eventData);
    }

    private void UpdateColor(PointerEventData eventData)
    {
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            wheelImage.rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPos
        );

        // If we don't have a readable texture, bail out
        if (wheelTexture == null || wheelImage == null)
            return;

        float width = wheelImage.rectTransform.rect.width;
        float height = wheelImage.rectTransform.rect.height;

        // Check if the pointer is inside the circular wheel area. This ignores clicks outside the circle
        // even if the image has no alpha transparency.
        float nx = localPos.x / (width * 0.5f);
        float ny = localPos.y / (height * 0.5f);
        if (Mathf.Sqrt(nx * nx + ny * ny) > 1f)
        {
            // outside circle -> ignore and hide selector
            if (selector != null)
                selector.gameObject.SetActive(false);
            return;
        }

        // inside circle -> show selector and sample
        if (selector != null)
        {
            selector.gameObject.SetActive(true);
            selector.localPosition = localPos;
        }

        // Convertir posición local en coordenadas UV (0..1)
        float u = (localPos.x + width / 2f) / width;
        float v = (localPos.y + height / 2f) / height;

        // Sample color using bilinear filtering for smoother results
        Color c = Color.white;
        try{
            c = wheelTexture.GetPixelBilinear(u, v);
        }catch(System.Exception){
            int x = Mathf.Clamp((int)(u * wheelTexture.width), 0, wheelTexture.width - 1);
            int y = Mathf.Clamp((int)(v * wheelTexture.height), 0, wheelTexture.height - 1);
            c = wheelTexture.GetPixel(x, y);
        }

        // Update preview UI if assigned
        if(previewImage != null){
            previewImage.color = c;
        }

        // Update selector image color if available
        if(selectorImage != null){
            selectorImage.color = c;
        } else if(selector != null){
            // if selector has no Image, try to set the color of its GameObject's Image (defensive)
            var img = selector.gameObject.GetComponent<Image>();
            if(img != null) img.color = c;
        }

        // Aplicarlo al coche usando detección por color clave (púrpura A349A4) y luego actualizar materiales
        if (mask != null)
        {
            // color clave (hex A349A4) -> RGB 163,73,164 -> normalizado 0..1
            Color keyColor = new Color(163f/255f, 73f/255f, 164f/255f);
            float tolerance = keyColorTolerance; // ajustable desde Inspector
            // Use the new helper that paints and applies in one call (idempotent/repeatable)
            mask.PaintAndApply(keyColor, tolerance, c, carRenderer, "preserveLuminance");
        }
    }
}

