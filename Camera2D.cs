using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ValleQuebrado.World;

/// <summary>Cámara 2D que sigue al jugador y no se sale de los bordes del mapa.</summary>
public sealed class Camera2D
{
    /// <summary>Centro de la cámara en coordenadas del mundo.</summary>
    public Vector2 Position { get; private set; }

    /// <summary>Escala entera para que los píxeles queden nítidos y sin costuras.</summary>
    public float Zoom { get; private set; } = 2f;

    /// <summary>Calcula el zoom para ver unas 11 filas de tiles en pantalla.</summary>
    public void UpdateZoom(Viewport vp)
        => Zoom = Math.Max(1, (int)MathF.Round(vp.Height / (TileMap.TileSize * 11f)));

    public void Follow(Vector2 target, Viewport vp, int mapPixelWidth, int mapPixelHeight)
    {
        float halfW = vp.Width / Zoom / 2f;
        float halfH = vp.Height / Zoom / 2f;

        float x = mapPixelWidth <= halfW * 2f ? mapPixelWidth / 2f : Math.Clamp(target.X, halfW, mapPixelWidth - halfW);
        float y = mapPixelHeight <= halfH * 2f ? mapPixelHeight / 2f : Math.Clamp(target.Y, halfH, mapPixelHeight - halfH);

        Position = new Vector2(MathF.Round(x * Zoom) / Zoom, MathF.Round(y * Zoom) / Zoom);
    }

    public Matrix GetMatrix(Viewport vp)
        => Matrix.CreateTranslation(-Position.X, -Position.Y, 0f)
         * Matrix.CreateScale(Zoom, Zoom, 1f)
         * Matrix.CreateTranslation(vp.Width / 2f, vp.Height / 2f, 0f);

    public Vector2 ScreenToWorld(Vector2 screen, Viewport vp)
        => (screen - new Vector2(vp.Width / 2f, vp.Height / 2f)) / Zoom + Position;

    public Rectangle VisibleArea(Viewport vp)
    {
        int w = (int)MathF.Ceiling(vp.Width / Zoom) + TileMap.TileSize;
        int h = (int)MathF.Ceiling(vp.Height / Zoom) + TileMap.TileSize;
        return new Rectangle((int)(Position.X - w / 2f), (int)(Position.Y - h / 2f), w, h);
    }
}
