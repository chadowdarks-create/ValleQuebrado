using Microsoft.Xna.Framework;

/// <summary>Tipo de arma o herramienta usada para atacar.</summary>
public enum CombatWeaponType
{
    MeleeWeapon,
    Tool
}

/// <summary>Datos de combate de un arma o herramienta.</summary>
public sealed record CombatWeapon(
    string Id,
    string Name,
    CombatWeaponType Type,
    float Damage,
    float VigorCost,
    float Range = 48f);

/// <summary>Entidad que puede recibir daño durante un combate.</summary>
public sealed class CombatTarget
{
    public CombatTarget(string name, float maxHealth, Vector2 position = default, float defense = 0f)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El objetivo debe tener un nombre.", nameof(name));
        if (maxHealth <= 0f)
            throw new ArgumentOutOfRangeException(nameof(maxHealth));
        if (defense < 0f)
            throw new ArgumentOutOfRangeException(nameof(defense));

        Name = name;
        MaxHealth = maxHealth;
        Health = maxHealth;
        Position = position;
        Defense = defense;
    }

    public string Name { get; }
    public float MaxHealth { get; }
    public float Health { get; private set; }
    public Vector2 Position { get; set; }
    public float Defense { get; set; }
    public bool IsDefeated => Health <= 0f;

    internal float ReceiveDamage(float amount)
    {
        float actualDamage = Math.Min(Health, Math.Max(0f, amount - Defense));
        Health -= actualDamage;
        return actualDamage;
    }
}

/// <summary>Resultado de intentar realizar un ataque.</summary>
public sealed record AttackResult(
    bool Succeeded,
    string Message,
    float DamageDealt = 0f,
    bool TargetDefeated = false);

/// <summary>
/// Gestiona Vigor y ataques cuerpo a cuerpo en calabozos. Las herramientas pueden
/// usarse como armas si se proporcionan con sus propios valores de daño y coste.
/// </summary>
public sealed class CombatManager
{
    /// <summary>Crea el sistema de combate para el personaje.</summary>
    public CombatManager(float maxVigor = 100f, float startingVigor = 100f)
    {
        if (maxVigor < 0f)
            throw new ArgumentOutOfRangeException(nameof(maxVigor));
        if (startingVigor < 0f || startingVigor > maxVigor)
            throw new ArgumentOutOfRangeException(nameof(startingVigor));

        MaxVigor = maxVigor;
        Vigor = startingVigor;
    }

    /// <summary>Vigor disponible para atacar.</summary>
    public float Vigor { get; private set; }

    /// <summary>Vigor máximo.</summary>
    public float MaxVigor { get; }

    /// <summary>Vigor recuperado por segundo fuera de un ataque.</summary>
    public float VigorRecoveryPerSecond { get; set; } = 10f;

    /// <summary>Distancia actual del personaje en el mundo.</summary>
    public Vector2 PlayerPosition { get; set; }

    /// <summary>Intenta ejecutar un ataque con arma o herramienta.</summary>
    public AttackResult Attack(CombatTarget target, CombatWeapon weapon)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(weapon);
        ValidateWeapon(weapon);

        if (target.IsDefeated)
            return new AttackResult(false, $"{target.Name} ya fue derrotado.");
        if (Vector2.Distance(PlayerPosition, target.Position) > weapon.Range)
            return new AttackResult(false, "El objetivo está fuera del alcance.");
        if (Vigor < weapon.VigorCost)
            return new AttackResult(false, "No hay suficiente Vigor para atacar.");

        Vigor = Math.Max(0f, Vigor - weapon.VigorCost);
        float damage = target.ReceiveDamage(weapon.Damage);
        string source = weapon.Type == CombatWeaponType.Tool ? "herramienta" : "arma";
        string message = $"Atacaste a {target.Name} con {weapon.Name} ({source}) e infligiste {damage:0.#} de daño.";
        return new AttackResult(true, message, damage, target.IsDefeated);
    }

    /// <summary>Recupera Vigor con el paso del tiempo, limitado al máximo.</summary>
    public void RecoverVigor(float deltaSeconds)
    {
        if (deltaSeconds <= 0f)
            return;
        Vigor = Math.Min(MaxVigor, Vigor + Math.Max(0f, VigorRecoveryPerSecond) * deltaSeconds);
    }

    /// <summary>Restaura una cantidad explícita de Vigor.</summary>
    public void RestoreVigor(float amount)
    {
        if (amount < 0f)
            throw new ArgumentOutOfRangeException(nameof(amount));
        Vigor = Math.Min(MaxVigor, Vigor + amount);
    }

    private static void ValidateWeapon(CombatWeapon weapon)
    {
        if (string.IsNullOrWhiteSpace(weapon.Id) || string.IsNullOrWhiteSpace(weapon.Name))
            throw new ArgumentException("El arma o herramienta debe tener ID y nombre.", nameof(weapon));
        if (weapon.Damage < 0f)
            throw new ArgumentOutOfRangeException(nameof(weapon), "El daño no puede ser negativo.");
        if (weapon.VigorCost < 0f)
            throw new ArgumentOutOfRangeException(nameof(weapon), "El coste de Vigor no puede ser negativo.");
        if (weapon.Range < 0f)
            throw new ArgumentOutOfRangeException(nameof(weapon), "El alcance no puede ser negativo.");
    }
}
