using Microsoft.Xna.Framework;

/// <summary>
/// Controla el movimiento del jugador con joystick virtual o con destino Tap-to-Tile.
/// Las posiciones y destinos se expresan en coordenadas del mundo.
/// </summary>
public sealed class PlayerController
{
    private Vector2 _joystickInput;
    private Vector2? _tileTarget;

    /// <summary>Posición actual del jugador en coordenadas del mundo.</summary>
    public Vector2 Position { get; set; }

    /// <summary>Velocidad de movimiento en unidades del mundo por segundo.</summary>
    public float Speed { get; set; } = 160f;

    /// <summary>Estamina disponible.</summary>
    public float Stamina { get; private set; } = 100f;

    /// <summary>Estamina máxima.</summary>
    public float MaxStamina { get; set; } = 100f;

    /// <summary>Consumo de estamina por segundo mientras el jugador se mueve.</summary>
    public float StaminaDrainPerSecond { get; set; } = 8f;

    /// <summary>Recuperación de estamina por segundo mientras el jugador está quieto.</summary>
    public float StaminaRecoveryPerSecond { get; set; } = 12f;

    /// <summary>Umbral de distancia para considerar alcanzado un destino.</summary>
    public float ArrivalDistance { get; set; } = 2f;

    /// <summary>Indica si hay un destino Tap-to-Tile activo.</summary>
    public bool HasTileTarget => _tileTarget.HasValue;

    /// <summary>Destino Tap-to-Tile actual, si existe.</summary>
    public Vector2? TileTarget => _tileTarget;

    /// <summary>
    /// Actualiza la dirección del joystick virtual. La magnitud se limita a 1,
    /// de modo que las diagonales no aumenten la velocidad.
    /// </summary>
    public void SetJoystickInput(Vector2 input)
    {
        _joystickInput = input.LengthSquared() > 1f ? Vector2.Normalize(input) : input;

        // El control directo del joystick cancela el destino automático.
        if (_joystickInput != Vector2.Zero)
            _tileTarget = null;
    }

    /// <summary>Establece el destino del jugador al tocar una posición del mundo.</summary>
    public void MoveTo(Vector2 worldPosition)
    {
        _tileTarget = worldPosition;
        _joystickInput = Vector2.Zero;
    }

    /// <summary>Cancela el movimiento Tap-to-Tile y detiene el joystick.</summary>
    public void Stop()
    {
        _joystickInput = Vector2.Zero;
        _tileTarget = null;
    }

    /// <summary>Avanza el movimiento y la recuperación o consumo de estamina.</summary>
    public void Update(float deltaSeconds)
    {
        if (deltaSeconds <= 0f)
            return;

        Vector2 direction = _joystickInput;
        bool movingToTarget = false;

        if (direction == Vector2.Zero && _tileTarget.HasValue)
        {
            Vector2 offset = _tileTarget.Value - Position;
            float distance = offset.Length();

            if (distance <= Math.Max(0f, ArrivalDistance))
            {
                Position = _tileTarget.Value;
                _tileTarget = null;
            }
            else
            {
                direction = offset / distance;
                movingToTarget = true;
            }
        }

        bool isMoving = direction != Vector2.Zero && Stamina > 0f && Speed > 0f;
        if (isMoving)
        {
            float distanceThisFrame = Speed * deltaSeconds;

            if (movingToTarget && _tileTarget.HasValue)
            {
                Vector2 offset = _tileTarget.Value - Position;
                float distance = offset.Length();
                if (distanceThisFrame >= distance)
                {
                    Position = _tileTarget.Value;
                    _tileTarget = null;
                }
                else
                {
                    Position += direction * distanceThisFrame;
                }
            }
            else
            {
                Position += direction * distanceThisFrame;
            }

            Stamina = Math.Max(0f, Stamina - Math.Max(0f, StaminaDrainPerSecond) * deltaSeconds);
        }
        else
        {
            Stamina = Math.Min(Math.Max(0f, MaxStamina),
                Stamina + Math.Max(0f, StaminaRecoveryPerSecond) * deltaSeconds);
        }

        // Mantiene el valor dentro del máximo incluso si MaxStamina cambió.
        Stamina = Math.Clamp(Stamina, 0f, Math.Max(0f, MaxStamina));
    }
}
