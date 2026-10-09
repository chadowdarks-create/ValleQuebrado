/// <summary>Modificadores que concede una bendición inicial.</summary>
public sealed record Blessing(
    string Id,
    string Name,
    string Description,
    float SpeedBonusPercent = 0f,
    float FatigueResistancePercent = 0f,
    float BrokenBasementLuckPercent = 0f);

/// <summary>
/// Administra las bendiciones iniciales del personaje y calcula sus modificadores
/// acumulados. La resistencia a la fatiga reduce el consumo de estamina.
/// </summary>
public sealed class BlessingsManager
{
    private readonly Dictionary<string, Blessing> _availableBlessings;
    private readonly HashSet<string> _selectedIds = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Bendiciones iniciales incluidas por defecto en Valle Quebrado.</summary>
    public static IReadOnlyList<Blessing> DefaultBlessings { get; } =
    [
        new Blessing("paso-ligero", "Paso Ligero",
            "Aumenta la velocidad de movimiento un 10%.", SpeedBonusPercent: 10f),
        new Blessing("aliento-sereno", "Aliento Sereno",
            "Reduce el consumo de estamina un 20%.", FatigueResistancePercent: 20f),
        new Blessing("fortuna-subterranea", "Fortuna Subterránea",
            "Aumenta un 15% la suerte en el Sótano Roto.", BrokenBasementLuckPercent: 15f)
    ];

    /// <summary>Crea el administrador con las bendiciones iniciales predeterminadas.</summary>
    public BlessingsManager() : this(DefaultBlessings) { }

    /// <summary>Crea el administrador con un catálogo personalizado.</summary>
    public BlessingsManager(IEnumerable<Blessing> blessings)
    {
        ArgumentNullException.ThrowIfNull(blessings);
        _availableBlessings = new Dictionary<string, Blessing>(StringComparer.OrdinalIgnoreCase);

        foreach (Blessing blessing in blessings)
        {
            ArgumentNullException.ThrowIfNull(blessing);
            if (string.IsNullOrWhiteSpace(blessing.Id))
                throw new ArgumentException("Cada bendición debe tener un identificador.", nameof(blessings));
            if (!_availableBlessings.TryAdd(blessing.Id, blessing))
                throw new ArgumentException($"El identificador '{blessing.Id}' está repetido.", nameof(blessings));
        }
    }

    /// <summary>Catálogo de bendiciones que se pueden elegir.</summary>
    public IReadOnlyCollection<Blessing> AvailableBlessings => _availableBlessings.Values;

    /// <summary>Bendiciones que tiene actualmente el personaje.</summary>
    public IReadOnlyList<Blessing> SelectedBlessings =>
        _selectedIds.Select(id => _availableBlessings[id]).ToArray();

    /// <summary>Máximo de bendiciones iniciales que puede elegir el personaje.</summary>
    public int SelectionLimit { get; set; } = 1;

    /// <summary>Bonificación de velocidad acumulada, expresada como fracción (0.10 = 10%).</summary>
    public float SpeedBonus => SelectedBlessings.Sum(blessing => blessing.SpeedBonusPercent) / 100f;

    /// <summary>Reducción del consumo de estamina acumulada, como fracción.</summary>
    public float FatigueResistance => SelectedBlessings.Sum(blessing => blessing.FatigueResistancePercent) / 100f;

    /// <summary>Bonificación de suerte en el Sótano Roto, como fracción.</summary>
    public float BrokenBasementLuck => SelectedBlessings.Sum(blessing => blessing.BrokenBasementLuckPercent) / 100f;

    /// <summary>Elige una bendición inicial disponible.</summary>
    public bool Select(string id)
    {
        if (!_availableBlessings.ContainsKey(id) || _selectedIds.Contains(id))
            return false;

        if (SelectionLimit < 1 || _selectedIds.Count >= SelectionLimit)
            return false;

        return _selectedIds.Add(id);
    }

    /// <summary>Quita una bendición seleccionada.</summary>
    public bool Remove(string id) => _selectedIds.Remove(id);

    /// <summary>Elimina todas las selecciones para permitir elegir de nuevo.</summary>
    public void Clear() => _selectedIds.Clear();

    /// <summary>Obtiene una bendición del catálogo por su identificador.</summary>
    public bool TryGetBlessing(string id, out Blessing? blessing) =>
        _availableBlessings.TryGetValue(id, out blessing);
}
