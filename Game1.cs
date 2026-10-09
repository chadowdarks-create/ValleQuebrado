using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using ValleQuebrado.UI;
using ValleQuebrado.World;

namespace ValleQuebrado;

/// <summary>
/// Mundo con cambio de zona: pisar una salida (tile brillante) usa WorldGraph/LocationManager.
/// Mitad izquierda = joystick virtual; mitad derecha = Tap-to-Tile.
/// </summary>
public class Game1 : Game
{
    private const float JoystickRadius = 90f;
    private const int HitboxSize = 20;

    // Ponlo en true para probar todas las zonas sin cumplir las condiciones del juego.
    private const bool DebugUnlockAll = false;

    private readonly GraphicsDeviceManager _graphics;
    private readonly PlayerController _player = new();
    private readonly Camera2D _camera = new();
    private readonly LocationManager _location = new();
    private readonly Dictionary<LocationId, TileMap> _maps = new();

    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;
    private TileMap _map = null!;

    private int? _joystickId;
    private Vector2 _joystickOrigin;
    private Vector2 _joystickPosition;

    private Point? _disarmedTile;
    private float _fade;
    private string _banner = string.Empty;
    private float _bannerTime;
    private string _toast = string.Empty;
    private float _toastTime;

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

        if (DebugUnlockAll)
        {
            WorldState s = _location.State;
            s.HasDungeonPermit = true;
            s.HasSewerKey = true;
            s.GiantBridgeRepaired = true;
            s.LowTide = true;
            s.GuildRank = 1;
            s.HighestBossFloorDefeated = 35;
            s.GeneralsDefeated = 7;
            s.Gold = 1000;
        }

        _map = GetMap(_location.Current);
        _player.Position = TileCenter(FarmMapFactory.SpawnTile);
        ShowBanner(_location.Current);
    }

    private TileMap GetMap(LocationId id)
    {
        if (!_maps.TryGetValue(id, out TileMap? map))
        {
            map = ZoneMapFactory.Build(id);
            _maps[id] = map;
        }
        return map;
    }

    private static Vector2 TileCenter(Point t)
        => new(t.X * TileMap.TileSize + TileMap.TileSize / 2f, t.Y * TileMap.TileSize + TileMap.TileSize / 2f);

    private void ShowBanner(LocationId id)
    {
        _banner = WorldGraph.Locations[id].Name;
        _bannerTime = 2.5f;
    }

    private Rectangle Hitbox(Vector2 p)
        => new((int)MathF.Floor(p.X) - HitboxSize / 2, (int)MathF.Floor(p.Y) - HitboxSize / 2, HitboxSize, HitboxSize);

    protected override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
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

        // Movimiento con colisión por ejes.
        Vector2 before = _player.Position;
        _player.Update(dt);
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

        // Salidas: se activan al pisarlas y se rearman al salir del tile.
        var tile = new Point((int)MathF.Floor(_player.Position.X / TileMap.TileSize),
                             (int)MathF.Floor(_player.Position.Y / TileMap.TileSize));
        if (_disarmedTile.HasValue && tile != _disarmedTile.Value)
            _disarmedTile = null;
        if (_disarmedTile is null)
            CheckWarp(tile);

        _fade = MathF.Max(0f, _fade - dt * 2.5f);
        _bannerTime = MathF.Max(0f, _bannerTime - dt);
        _toastTime = MathF.Max(0f, _toastTime - dt);

        _camera.Follow(_player.Position, vp, _map.PixelWidth, _map.PixelHeight);
        base.Update(gameTime);
    }

    private void CheckWarp(Point tile)
    {
        WarpResult result = _location.TryWarp(tile);

        if (result.Success)
        {
            _map = GetMap(result.Destination);
            _player.Stop();
            _player.Position = TileCenter(result.SpawnTile);
            _disarmedTile = result.SpawnTile;
            _fade = 1f;
            ShowBanner(result.Destination);
        }
        else if (!string.IsNullOrEmpty(result.Message))
        {
            _toast = result.Message;
            _toastTime = 3f;
            _disarmedTile = tile;
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        Viewport vp = GraphicsDevice.Viewport;
        float time = (float)gameTime.TotalGameTime.TotalSeconds;
        GraphicsDevice.Clear(new Color(20, 30, 22));

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.GetMatrix(vp));
        _map.Draw(_spriteBatch, _pixel, _camera.VisibleArea(vp), time);
        DrawPlayer();
        _spriteBatch.End();

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        DrawHud(vp);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawHud(Viewport vp)
    {
        // Estamina.
        float ratio = _player.MaxStamina > 0f ? _player.Stamina / _player.MaxStamina : 0f;
        _spriteBatch.Draw(_pixel, new Rectangle(20, 20, 200, 16), Color.Black * 0.6f);
        _spriteBatch.Draw(_pixel, new Rectangle(20, 20, (int)(200 * ratio), 16), new Color(90, 200, 120));

        // Joystick.
        if (_joystickId is not null)
        {
            int r = (int)JoystickRadius;
            _spriteBatch.Draw(_pixel,
                new Rectangle((int)_joystickOrigin.X - r, (int)_joystickOrigin.Y - r, r * 2, r * 2), Color.White * 0.15f);
            _spriteBatch.Draw(_pixel,
                new Rectangle((int)_joystickPosition.X - 25, (int)_joystickPosition.Y - 25, 50, 50), Color.White * 0.5f);
        }

        // Nombre de la zona (arriba) y mensajes (abajo).
        if (_bannerTime > 0f)
            DrawCenteredText(vp, _banner, 70, new Color(255, 226, 140), MathF.Min(1f, _bannerTime));
        if (_toastTime > 0f)
            DrawCenteredText(vp, _toast, vp.Height - 130, Color.White, MathF.Min(1f, _toastTime));

        // Fundido al cambiar de zona.
        if (_fade > 0f)
            _spriteBatch.Draw(_pixel, new Rectangle(0, 0, vp.Width, vp.Height), Color.Black * _fade);
    }

    private void DrawCenteredText(Viewport vp, string text, int y, Color color, float alpha)
    {
        int scale = Math.Max(2, vp.Height / 220);
        while (scale > 2 && PixelFont.Measure(text, scale) > vp.Width - 80)
            scale--;

        int w = PixelFont.Measure(text, scale);
        int x = (vp.Width - w) / 2;
        const int pad = 16;
        _spriteBatch.Draw(_pixel, new Rectangle(x - pad, y - pad, w + pad * 2, 5 * scale + pad * 2), Color.Black * (0.7f * alpha));
        PixelFont.Draw(_spriteBatch, _pixel, text, new Vector2(x, y), scale, color * alpha);
    }

    private void DrawPlayer()
    {
        int px = (int)MathF.Floor(_player.Position.X);
        int py = (int)MathF.Floor(_player.Position.Y);

        _spriteBatch.Draw(_pixel, new Rectangle(px - 9, py + 4, 18, 6), Color.Black * 0.3f);
        _spriteBatch.Draw(_pixel, new Rectangle(px - 8, py - 6, 16, 14), new Color(70, 110, 190));
        _spriteBatch.Draw(_pixel, new Rectangle(px - 7, py - 20, 14, 14), new Color(238, 200, 160));
        _spriteBatch.Draw(_pixel, new Rectangle(px - 7, py - 22, 14, 6), new Color(90, 56, 36));
    }
}
