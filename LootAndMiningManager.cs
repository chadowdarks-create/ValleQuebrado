using System.Collections.ObjectModel;

namespace ValleQuebrado.Economy;

/// <summary>Regla de extracción para un nodo mineral.</summary>
public sealed record MiningNode(
    string MineralId,
    string Location,
    int DifficultyLevel,
    string RequiredTool);

/// <summary>Tabla de botín de una criatura y las zonas donde aparece.</summary>
public sealed record CreatureDrop(
    string CreatureName,
    string SourceTheme,
    string Location,
    IReadOnlyList<string> Drops);

public sealed record MiningResult(bool Success, string Message, int QuantityAdded = 0);

/// <summary>Reglas para extraer minerales y consultar botín de criaturas.</summary>
public static class LootAndMiningManager
{
    private static readonly Dictionary<string, MiningNode> MiningData = new(StringComparer.OrdinalIgnoreCase)
    {
        ["cobre_rocoso"] = new("cobre_rocoso", "Sótano Roto", 1, "Pico de Cobre"),
        ["hierro_espectral"] = new("hierro_espectral", "Minas de Almas", 2, "Pico de Hierro"),
        ["obsidiana_vacia"] = new("obsidiana_vacia", "Fortaleza del Vacío", 5, "Pico del Vacío")
    };

    private static readonly IReadOnlyList<CreatureDrop> CreatureData = Array.AsReadOnly(new[]
    {
        new CreatureDrop("Slime de Sombra", "Stardew / Isekai", "Sótano Roto y Alcantarillas",
            Array.AsReadOnly(new[] { "Gelatina pegajosa", "Esencia de alma menor" })),
        new CreatureDrop("Esqueleto Errante", "Graveyard Keeper", "Cripta de Calaveras",
            Array.AsReadOnly(new[] { "Huesos rotos", "Polvo de hueso" })),
        new CreatureDrop("Caballero del Vacío", "Elden Ring", "Pisos de Jefe en Minas de Almas",
            Array.AsReadOnly(new[] { "Trofeo de General", "Alma pura de combate" }))
    });

    public static IReadOnlyDictionary<string, MiningNode> MiningRules { get; } =
        new ReadOnlyDictionary<string, MiningNode>(MiningData);

    public static IReadOnlyList<CreatureDrop> CreatureDatabase => CreatureData;

    /// <summary>Intenta extraer el mineral y depositarlo en el gestor de recursos.</summary>
    public static MiningResult TryMine(
        string mineralId,
        string location,
        string equippedTool,
        int miningSkill,
        ResourceManager resources,
        int quantity = 1)
    {
        ArgumentNullException.ThrowIfNull(resources);
        if (quantity < 1)
            return new MiningResult(false, "La cantidad debe ser positiva.");
        if (!MiningData.TryGetValue(mineralId, out MiningNode? node))
            return new MiningResult(false, "No hay una regla de extracción para ese mineral.");
        if (!StringComparer.OrdinalIgnoreCase.Equals(node.Location, location))
            return new MiningResult(false, $"Ese mineral no se encuentra en {location}.");
        if (miningSkill < node.DifficultyLevel)
            return new MiningResult(false, $"Necesitas nivel de minería {node.DifficultyLevel}.");
        if (!StringComparer.OrdinalIgnoreCase.Equals(node.RequiredTool, equippedTool))
            return new MiningResult(false, $"Necesitas {node.RequiredTool} para extraerlo.");

        resources.Gather(node.MineralId, quantity);
        return new MiningResult(true, $"Extrajiste {quantity} unidad(es) de {node.MineralId}.", quantity);
    }

    /// <summary>Devuelve las tablas de botín disponibles para una criatura en una zona.</summary>
    public static IReadOnlyList<string> GetCreatureDrops(string creatureName, string location)
    {
        CreatureDrop? creature = CreatureData.FirstOrDefault(entry =>
            StringComparer.OrdinalIgnoreCase.Equals(entry.CreatureName, creatureName));
        if (creature is null || string.IsNullOrWhiteSpace(location))
            return Array.Empty<string>();

        bool locationMatches = creature.Location.Split(" y ", StringSplitOptions.TrimEntries)
            .Any(area => area.Contains(location, StringComparison.OrdinalIgnoreCase) ||
                         location.Contains(area, StringComparison.OrdinalIgnoreCase));
        return locationMatches ? creature.Drops : Array.Empty<string>();
    }
}
