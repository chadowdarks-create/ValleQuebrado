using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using ValleQuebrado.World;

namespace ValleQuebrado;

/// <summary>
/// Granja con tiles, cámara que sigue al jugador y colisiones.
/// Mitad izquierda = joystick virtual; mitad derecha = Tap-to-Tile.
/// </summary>
public class Game1 : Game
{
    private const float JoystickRadius = 90f;
    private const int HitboxSize = 20;

    private readonly GraphicsDeviceManager _graphics;
    private readonly PlayerController _player = new();
    private readonly Camera2D _camera = new();

    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;
    private TileMap _map = null!;

    private int? _joystickId;
    private Vector2 _joystickOrigin;
    private Vector2 _joystickPosition;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            IsFullScreen = true,
            SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight
        };
        IsMouseVisible = false;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _map = FarmMapFactory.Build();
        Point s = FarmMapFactory.SpawnTile;
        _player.Position = new Vector2(s.X * TileMap.TileSize + TileMap.TileSize / 2f,
                                       s.Y * TileMap.TileSize + TileMap.TileSize / 2f);
    }

    private Rectangle Hitbox(Vector2 p)
        => new((int)MathF.Floor(p.X) - HitboxSize / 2, (int)MathF.Floor(p.Y) - HitboxSize / 2, HitboxSize, HitboxSize);

    protected override void Update(GameTime gameTime)
    {
        Viewport vp = GraphicsDevice.Viewport;
        _camera.UpdateZoom(vp);

        foreach (TouchLocation touch in TouchPanel.GetState())
        {
            switch (touch.State)
            {
                case TouchLocationState.Pressed:
                    if (touch.Position.X < vp.Width / 2f && _joystickId is null)
                    {
                        _joystickId = touch.Id;
                        _joystickOrigin = touch.Position;
                        _joystickPosition = touch.Position;
                    }
                    else if (touch.Position.X >= vp.Width / 2f)
                    {
                        _player.MoveTo(_camera.ScreenToWorld(touch.Position, vp));
                    }
                    break;

                case TouchLocationState.Moved:
                    if (touch.Id == _joystickId)
                        _joystickPosition = touch.Position;
                    break;

                case TouchLocationState.Released:
                    if (touch.Id == _joystickId)
                    {
                        _joystickId = null;
                        _player.SetJoystickInput(Vector2.Zero);
                    }
                    break;
            }
        }

        if (_joystickId is not null)
            _player.SetJoystickInput((_joystickPosition - _joystickOrigin) / JoystickRadius);

        // Movimiento con colisión por ejes (permite deslizarse junto a las paredes).
        Vector2 before = _player.Position;
        _player.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        Vector2 after = _player.Position;

        Vector2 resolved = before;
        var tryX = new Vector2(after.X, before.Y);
        if (!_map.IsBlocked(Hitbox(tryX)))
            resolved.X = tryX.X;
        var tryY = new Vector2(resolved.X, after.Y);
        if (!_map.IsBlocked(Hitbox(tryY)))
            resolved.Y = tryY.Y;

        _player.Position = resolved;
        if (after != before && resolved == before && _player.HasTileTarget)
            _player.Stop();

        _camera.Follow(_player.Position, vp, _map.PixelWidth, _map.PixelHeight);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        Viewport vp = GraphicsDevice.Viewport;
        float time = (float)gameTime.TotalGameTime.TotalSeconds;
        GraphicsDevice.Clear(new Color(20, 30, 22));

        // Mundo (con cámara).
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.GetMatrix(vp));
        _map.Draw(_spriteBatch, _pixel, _camera.VisibleArea(vp), time);
        DrawPlayer();
        _spriteBatch.End();

        // Interfaz (sin cámara).
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        float ratio = _player.MaxStamina > 0f ? _player.Stamina / _player.MaxStamina : 0f;
        _spriteBatch.Draw(_pixel, new Rectangle(20, 20, 200, 16), Color.Black * 0.6f);
        _spriteBatch.Draw(_pixel, new Rectangle(20, 20, (int)(200 * ratio), 16), new Color(90, 200, 120));

        if (_joystickId is not null)
        {
            int r = (int)JoystickRadius;
            _spriteBatch.Draw(_pixel,
                new Rectangle((int)_joystickOrigin.X - r, (int)_joystickOrigin.Y - r, r * 2, r * 2), Color.White * 0.15f);
            _spriteBatch.Draw(_pixel,
                new Rectangle((int)_joystickPosition.X - 25, (int)_joystickPosition.Y - 25, 50, 50), Color.White * 0.5f);
        }
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawPlayer()
    {
        int px = (int)MathF.Floor(_player.Position.X);
        int py = (int)MathF.Floor(_player.Position.Y);

        _spriteBatch.Draw(_pixel, new Rectangle(px - 9, py + 4, 18, 6), Color.Black * 0.3f);          // sombra
        _spriteBatch.Draw(_pixel, new Rectangle(px - 8, py - 6, 16, 14), new Color(70, 110, 190));   // cuerpo
        _spriteBatch.Draw(_pixel, new Rectangle(px - 7, py - 20, 14, 14), new Color(238, 200, 160)); // cabeza
        _spriteBatch.Draw(_pixel, new Rectangle(px - 7, py - 22, 14, 6), new Color(90, 56, 36));     // pelo
    }
}
