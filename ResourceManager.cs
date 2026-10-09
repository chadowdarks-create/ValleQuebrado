using System.Collections.ObjectModel;

namespace ValleQuebrado.Economy;

public enum ResourceCategory
{
    Wood, Mineral, MonsterDrop, BossTrophy, SoulMaterial
}

/// <summary>Definición de un material recolectable en el mundo.</summary>
public sealed record ResourceItem(
    string Id,
    string Name,
    ResourceCategory Category,
    string SourceDescription);

/// <summary>Catálogo base de materiales, botines, trofeos y recursos de alma.</summary>
public static class ResourceDatabase
{
    private static readonly Dictionary<string, ResourceItem> ResourceData = new(StringComparer.OrdinalIgnoreCase)
    {
        ["madera_normal"] = new("madera_normal", "Madera de Pino", ResourceCategory.Wood,
            "Se obtiene al talar árboles comunes en la Granja o el Bosque."),
        ["madera_espectral"] = new("madera_espectral", "Madera Espectral", ResourceCategory.Wood,
            "Se obtiene en el Bosque Espectral con hachas mejoradas."),
        ["mineral_hierro"] = new("mineral_hierro", "Mineral de Hierro", ResourceCategory.Mineral,
            "Se obtiene picando vetas en las Montañas o el Sótano Roto."),
        ["lingote_magma"] = new("lingote_magma", "Lingote de Magma", ResourceCategory.Mineral,
            "Fundido en la Fragua del Cráter con minerales de las profundidades."),
        ["huesos_monstruo"] = new("huesos_monstruo", "Huesos Rotos", ResourceCategory.MonsterDrop,
            "Soltados por esqueletos y criaturas en las criptas."),
        ["piel_bestia"] = new("piel_bestia", "Piel de Bestia de Guerra", ResourceCategory.MonsterDrop,
            "Obtenida de criaturas salvajes en los pasos de montaña."),
        ["alma_menor"] = new("alma_menor", "Esencia de Alma Cautiva", ResourceCategory.SoulMaterial,
            "Liberada al derrotar enemigos comunes en los calabozos procedurales."),
        ["trofeo_general"] = new("trofeo_general", "Alma de General de Qixaroth", ResourceCategory.BossTrophy,
            "Obtenida al derrotar a uno de los 7 Generales en los pisos de jefe.")
    };

    public static IReadOnlyDictionary<string, ResourceItem> Catalog { get; } =
        new ReadOnlyDictionary<string, ResourceItem>(ResourceData);

    public static bool TryGet(string id, out ResourceItem? item) => ResourceData.TryGetValue(id, out item);
}

/// <summary>Inventario de recursos con operaciones para recolectar y consumir materiales.</summary>
public sealed class ResourceManager
{
    private readonly Dictionary<string, int> _quantities = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Cantidades actuales por ID de recurso.</summary>
    public IReadOnlyDictionary<string, int> Quantities =>
        new ReadOnlyDictionary<string, int>(_quantities);

    /// <summary>Notifica un recurso recolectado y devuelve la cantidad total resultante.</summary>
    public int Gather(string resourceId, int quantity = 1)
    {
        if (quantity < 1)
            throw new ArgumentOutOfRangeException(nameof(quantity), "La cantidad recolectada debe ser positiva.");
        if (!ResourceDatabase.Catalog.ContainsKey(resourceId) &&
            !AdvancedMineralCatalog.AllMinerals.ContainsKey(resourceId))
            return GetQuantity(resourceId);

        _quantities.TryGetValue(resourceId, out int current);
        int total = checked(current + quantity);
        _quantities[resourceId] = total;
        return total;
    }

    /// <summary>Consume la cantidad solicitada; si no alcanza, no modifica el inventario.</summary>
    public bool TryConsume(string resourceId, int quantity = 1)
    {
        if (quantity < 1)
            return false;
        if (!_quantities.TryGetValue(resourceId, out int current) || current < quantity)
            return false;

        int remaining = current - quantity;
        if (remaining == 0)
            _quantities.Remove(resourceId);
        else
            _quantities[resourceId] = remaining;
        return true;
    }

    public int GetQuantity(string resourceId) =>
        _quantities.TryGetValue(resourceId, out int quantity) ? quantity : 0;
}
