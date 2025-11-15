from PIL import Image
import argparse
import os
import math


def hex_to_rgb(hex_color):
    """Convierte un color hexadecimal (como '#A349A4') a tupla (r, g, b) con valores 0-255."""
    hex_color = hex_color.lstrip('#')
    if len(hex_color) != 6:
        raise ValueError("hex_color debe tener 6 dígitos, por ejemplo '#A349A4'")
    return tuple(int(hex_color[i:i+2], 16) for i in (0, 2, 4))


def _color_distance_normalized(c1, c2):
    """Distancia euclidiana normalizada entre dos colores RGB (0-255), resultado en [0,1]."""
    dr = c1[0] - c2[0]
    dg = c1[1] - c2[1]
    db = c1[2] - c2[2]
    dist = math.sqrt(dr * dr + dg * dg + db * db)
    max_dist = math.sqrt(3 * (255.0 ** 2))
    return dist / max_dist


def ensure_parent_dir(path):
    """Crea el directorio padre si no existe."""
    d = os.path.dirname(path)
    if d and not os.path.exists(d):
        os.makedirs(d, exist_ok=True)


def generate_mask(input_path, mask_output_path, key_rgb, tolerance=0.12):
    """Genera una máscara PNG donde los píxeles cercanos a `key_rgb` quedan blancos (alpha=255),
    y el resto transparentes. Esta máscara se guarda y puede reutilizarse.

    - `key_rgb`: tupla (r,g,b) 0-255
    - `tolerance`: valor entre 0..1 (0 exacto, 1 cualquier píxel)
    
    Retorna: (matched_pixels, min_distance, max_distance)
    """
    img = Image.open(input_path).convert('RGBA')
    px = img.load()
    w, h = img.size

    mask = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    mpx = mask.load()

    matched = 0
    min_d = 1.0
    max_d = 0.0

    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            d = _color_distance_normalized((r, g, b), key_rgb)
            min_d = min(min_d, d)
            max_d = max(max_d, d)
            if d <= tolerance:
                mpx[x, y] = (255, 255, 255, 255)  # Blanco = área a pintar
                matched += 1
            else:
                mpx[x, y] = (0, 0, 0, 0)  # Transparente = no pintar

    ensure_parent_dir(mask_output_path)
    mask.save(mask_output_path, format='PNG')
    print(f"✓ Máscara guardada en: {mask_output_path}")
    print(f"  Píxeles detectados: {matched} de {w*h} ({100*matched/(w*h):.2f}%)")
    print(f"  Distancia min: {min_d:.4f}, max: {max_d:.4f}")
    return matched, min_d, max_d


def generate_overlay(input_path, overlay_output_path, key_rgb, tolerance=0.12, paint_rgb=(255, 0, 0)):
    """Genera una copia del atlas donde los píxeles cercanos a `key_rgb` son reemplazados
    por `paint_rgb` (manteniendo alpha original). Útil para preview.
    """
    img = Image.open(input_path).convert('RGBA')
    px = img.load()
    w, h = img.size

    matched = 0
    min_d = 1.0
    max_d = 0.0

    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            d = _color_distance_normalized((r, g, b), key_rgb)
            min_d = min(min_d, d)
            max_d = max(max_d, d)
            if d <= tolerance:
                px[x, y] = (paint_rgb[0], paint_rgb[1], paint_rgb[2], a)
                matched += 1

    ensure_parent_dir(overlay_output_path)
    img.save(overlay_output_path, format='PNG')
    print(f"✓ Overlay guardado en: {overlay_output_path}")
    print(f"  Píxeles pintados: {matched} de {w*h} ({100*matched/(w*h):.2f}%)")
    print(f"  Distancia min: {min_d:.4f}, max: {max_d:.4f}")
    return matched, min_d, max_d


def apply_mask_to_image(atlas_path, mask_path, output_path, paint_rgb):
    """Aplica una máscara previamente guardada a un atlas, pintando las regiones marcadas.
    
    - atlas_path: imagen original
    - mask_path: máscara (blanco = pintar, transparente/negro = no pintar)
    - output_path: resultado con color aplicado
    - paint_rgb: tupla (r,g,b) del color a aplicar
    """
    atlas = Image.open(atlas_path).convert('RGBA')
    mask = Image.open(mask_path).convert('RGBA')
    
    if atlas.size != mask.size:
        raise ValueError(f"Atlas y máscara tienen tamaños diferentes: {atlas.size} vs {mask.size}")
    
    apx = atlas.load()
    mpx = mask.load()
    w, h = atlas.size
    
    painted = 0
    for y in range(h):
        for x in range(w):
            mr, mg, mb, ma = mpx[x, y]
            # Si el píxel de la máscara es blanco (255) o tiene alpha > 128, pintamos
            if mr > 128 or ma > 128:
                r, g, b, a = apx[x, y]
                apx[x, y] = (paint_rgb[0], paint_rgb[1], paint_rgb[2], a)
                painted += 1
    
    ensure_parent_dir(output_path)
    atlas.save(output_path, format='PNG')
    print(f"✓ Atlas repintado guardado en: {output_path}")
    print(f"  Píxeles repintados: {painted} de {w*h} ({100*painted/(w*h):.2f}%)")
    return painted


def main():
    parser = argparse.ArgumentParser(
        description='Herramienta para generar máscara de color púrpura y aplicar recoloreo.',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Ejemplos de uso:

  1. Generar máscara del color púrpura (solo una vez):
     python Tools/map_purple.py atlas.png Assets/Textures/Masks/purple_mask.png -t 0.12

  2. Generar máscara + overlay de preview con color rojo:
     python Tools/map_purple.py atlas.png Assets/Textures/Masks/purple_mask.png -t 0.12 --paint "#FF0000" --overlay-output preview_red.png

  3. Aplicar máscara existente para repintar con otro color:
     python Tools/map_purple.py atlas.png output.png --apply-mask Assets/Textures/Masks/purple_mask.png --paint "#00FF00"
        """)
    
    parser.add_argument('input', help='Ruta a la imagen de entrada (atlas)')
    parser.add_argument('output', help='Ruta de salida (máscara o atlas repintado)')
    parser.add_argument('--tolerance', '-t', type=float, default=0.12,
                        help='Tolerancia de coincidencia (0..1). Por defecto: 0.12')
    parser.add_argument('--paint', '-p', type=str, default=None,
                        help='Color de pintura en hex (ej. "#FF0000")')
    parser.add_argument('--overlay-output', '-o', type=str, default=None,
                        help='Ruta de salida para el overlay de preview')
    parser.add_argument('--apply-mask', '-m', type=str, default=None,
                        help='Aplicar una máscara existente en lugar de generarla')
    parser.add_argument('--key-color', '-k', type=str, default='#A349A4',
                        help='Color clave a detectar en hex. Por defecto: #A349A4 (púrpura)')

    args = parser.parse_args()

    # Modo 1: Aplicar máscara existente
    if args.apply_mask:
        if not args.paint:
            print("Error: --apply-mask requiere --paint para especificar el color")
            return
        try:
            paint_rgb = hex_to_rgb(args.paint)
        except Exception as e:
            print(f"Error parseando --paint '{args.paint}': {e}")
            return
        
        print(f"Aplicando máscara '{args.apply_mask}' a '{args.input}' con color {args.paint}")
        apply_mask_to_image(args.input, args.apply_mask, args.output, paint_rgb)
        return

    # Modo 2: Generar máscara nueva
    key_rgb = hex_to_rgb(args.key_color)
    print(f"Generando máscara de '{args.input}' — key={args.key_color}, tolerance={args.tolerance}")
    matched, min_d, max_d = generate_mask(args.input, args.output, key_rgb, tolerance=args.tolerance)
    
    if matched == 0:
        print(f"⚠️  Advertencia: No se encontraron píxeles coincidentes. Prueba aumentar --tolerance o verificar --key-color")

    # Si se solicita overlay de preview
    if args.paint and args.overlay_output:
        try:
            paint_rgb = hex_to_rgb(args.paint)
        except Exception as e:
            print(f"Error parseando --paint '{args.paint}': {e}")
            return
        print(f"\nGenerando overlay de preview con color {args.paint}...")
        generate_overlay(args.input, args.overlay_output, key_rgb, tolerance=args.tolerance, paint_rgb=paint_rgb)


if __name__ == '__main__':
    main()
