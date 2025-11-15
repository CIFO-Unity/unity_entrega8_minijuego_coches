# Herramienta de Máscara de Color Púrpura

Esta herramienta permite generar y reutilizar máscaras de los píxeles de color púrpura en el atlas del coche, para aplicar cambios de color de forma eficiente en Unity sin tener que detectar los píxeles cada vez.

## Requisitos

```powershell
pip install Pillow
```

## Uso

### 1. Generar la máscara permanente (solo una vez)

Esta máscara identifica todos los píxeles púrpura del atlas y se guardará para reutilizarla:

```powershell
python Tools\map_purple.py "ruta\al\atlas.png" "Assets\Textures\Masks\purple_mask.png" -t 0.12
```

**Parámetros:**
- Primer argumento: ruta al atlas original
- Segundo argumento: ruta donde guardar la máscara
- `-t` o `--tolerance`: tolerancia de detección (0.0 = exacto, 1.0 = todo). Default: 0.12

**Salida:**
- `purple_mask.png`: máscara donde blanco = píxeles púrpura, transparente = resto
- Estadísticas en consola: número de píxeles detectados y distancias

### 2. Generar overlay de preview (opcional)

Para ver cómo quedará el atlas con un color aplicado antes de importarlo en Unity:

```powershell
python Tools\map_purple.py "ruta\al\atlas.png" "Assets\Textures\Masks\purple_mask.png" -t 0.12 --paint "#FF0000" --overlay-output "Assets\Textures\Masks\preview_red.png"
```

**Parámetros adicionales:**
- `--paint` o `-p`: color en hex para el overlay (ej: "#FF0000" = rojo)
- `--overlay-output` o `-o`: ruta donde guardar el preview

**Salida:**
- `purple_mask.png`: la máscara (igual que antes)
- `preview_red.png`: atlas con los píxeles púrpura reemplazados por el color especificado

### 3. Aplicar máscara existente con nuevo color

Una vez tienes la máscara guardada, puedes aplicarla rápidamente con cualquier color:

```powershell
python Tools\map_purple.py "ruta\al\atlas.png" "output\nuevo_atlas.png" --apply-mask "Assets\Textures\Masks\purple_mask.png" --paint "#00FF00"
```

**Parámetros:**
- `--apply-mask` o `-m`: ruta a la máscara previamente generada
- `--paint` o `-p`: color nuevo a aplicar (requerido con `--apply-mask`)

## Configuración en Unity

### Paso 1: Importar y configurar la máscara

1. Copia `Assets\Textures\Masks\purple_mask.png` a tu proyecto Unity
2. **Selecciona la textura** en el Project
3. **En el Inspector**, configura estos parámetros:
   - ✅ **Read/Write Enabled**: **ACTIVAR** (crítico - sin esto dará error)
   - **Texture Type**: Default o Sprite (2D and UI)
   - **Compression**: None (recomendado para máscara)
   - **Filter Mode**: Point (no filter) o Bilinear
4. **Haz clic en "Apply"** en la parte inferior del Inspector

⚠️ **Importante**: Si no activas "Read/Write Enabled", obtendrás el error:
```
UnityException: Texture 'purple_mask' is not readable
```

### Paso 2: Asignar la máscara en AtlasColorMask

1. Selecciona el GameObject que tiene el componente `AtlasColorMask`
2. En el Inspector, arrastra `purple_mask.png` al campo **Purple Mask**
3. Asegúrate de que **Atlas Texture** sigue apuntando al atlas original

### Paso 3: Comportamiento automático

Con la máscara asignada:
- `ColorPicker` llamará automáticamente a `PaintAndApply()`
- Si `purpleMask` está asignado → usa la máscara (rápido, reutilizable)
- Si `purpleMask` es null → detecta por color cada vez (más lento)

## Flujo de trabajo recomendado

### Primera vez (setup)

1. Ejecuta el script para generar la máscara:
   ```powershell
   python Tools\map_purple.py "C:\ruta\atlas.png" "Assets\Textures\Masks\purple_mask.png" -t 0.12 --paint "#FF0000" --overlay-output "preview.png"
   ```

2. Verifica el `preview.png` para confirmar que los píxeles detectados son correctos

3. Importa `purple_mask.png` en Unity y asígnalo al componente `AtlasColorMask`

### Uso diario

- El usuario selecciona colores en el Color Picker
- La máscara se reutiliza automáticamente → sin detección, cambio instantáneo
- Para resetear: llama `AtlasColorMask.ResetRuntimeAtlas()` o reinicia la escena

## Parámetros de detección

### Tolerancia (`--tolerance`)

- **0.0 - 0.05**: solo coincidencias exactas (puede fallar con compresión JPEG/sRGB)
- **0.08 - 0.12**: recomendado para PNG con ligera variación
- **0.15 - 0.20**: detecta variantes más amplias del color
- **> 0.3**: puede detectar falsos positivos

### Color clave (`--key-color`)

Por defecto: `#A349A4` (púrpura especificado)

Para cambiar:
```powershell
python Tools\map_purple.py "atlas.png" "mask.png" --key-color "#FF00FF" -t 0.12
```

## Ejemplos completos

### Ejemplo 1: Generar máscara desde atlas real

```powershell
python Tools\map_purple.py "C:\Users\felix\Downloads\PCC_TextureAtlas_Purpura.png" "Assets\Textures\Masks\purple_mask.png" -t 0.12
```

### Ejemplo 2: Preview con color verde

```powershell
python Tools\map_purple.py "C:\ejemplo.png" "Assets\Textures\Masks\purple_mask.png" -t 0.12 --paint "#00FF00" --overlay-output "Assets\Textures\Masks\preview_verde.png"
```

### Ejemplo 3: Aplicar máscara con azul

```powershell
python Tools\map_purple.py "C:\ejemplo.png" "output_azul.png" --apply-mask "Assets\Textures\Masks\purple_mask.png" --paint "#0000FF"
```

## Solución de problemas

### "No se encontraron píxeles coincidentes (matched=0)"

- Aumenta `--tolerance` (prueba 0.15 o 0.20)
- Verifica que el `--key-color` sea correcto
- Comprueba que el atlas tiene el color púrpura esperado

### "Máscara detecta demasiados píxeles"

- Reduce `--tolerance` (prueba 0.08 o 0.05)
- Verifica el `minDist` en la salida → si es > 0.1, el color clave puede no coincidir

### La máscara no se aplica en Unity

1. **Error "Texture is not readable"**:
   - Selecciona `purple_mask.png` en el Project
   - En el Inspector, activa **Read/Write Enabled** ✓
   - Haz clic en **Apply**
   
2. Verifica que `purpleMask` está asignado en el Inspector del componente `AtlasColorMask`

3. Comprueba la consola de Unity para otros errores

## Estructura de archivos

```
Tools/
  map_purple.py          # Script principal
  README.md             # Esta documentación

Assets/
  Textures/
    Masks/
      purple_mask.png   # Máscara generada (importar en Unity)
      preview_red.png   # Preview opcional
```
