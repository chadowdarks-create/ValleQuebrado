namespace ValleQuebrado.Crafting;

public enum ItemRarity
{
    Comun, Raro, Epico, Unico, Legendario, Mitologico
}

/// <summary>Equipo creado con rareza, estadísticas y texto propio.</summary>
public sealed class CraftedEquipment
{
    public required string ItemName { get; init; }
    public required string CrafterName { get; init; }
    public ItemRarity Rarity { get; init; }
    public int BaseAttackOrDefense { get; init; }
    public Dictionary<string, int> StatBonuses { get; } = new(StringComparer.OrdinalIgnoreCase);
    public string UniqueFlavorText { get; internal set; } = string.Empty;
}

/// <summary>Forja equipo y determina su rareza según destreza y materiales especiales.</summary>
public static class OvergearedCraftingManager
{
    /// <summary>Genera una pieza de equipo con bonificaciones basadas en su rareza.</summary>
    public static CraftedEquipment ForgeItem(
        string itemName,
        string crafter,
        int playerDexterity,
        IReadOnlyCollection<string> rareMaterials)
    {
        if (string.IsNullOrWhiteSpace(itemName))
            throw new ArgumentException("El equipo debe tener un nombre.", nameof(itemName));
        if (string.IsNullOrWhiteSpace(crafter))
            throw new ArgumentException("El artesano debe tener un nombre.", nameof(crafter));
        ArgumentNullException.ThrowIfNull(rareMaterials);

        ItemRarity rarity = RollRarity(playerDexterity, rareMaterials.Count);
        var equipment = new CraftedEquipment
        {
            ItemName = itemName,
            CrafterName = crafter,
            Rarity = rarity,
            BaseAttackOrDefense = 15 + (int)rarity * 10
        };
        ApplyRarityBonuses(equipment);
        return equipment;
    }

    private static ItemRarity RollRarity(int dexterity, int rareMaterialCount)
    {
        int score = Math.Max(0, dexterity) + (rareMaterialCount * 25) + Random.Shared.Next(1, 100);
        if (score > 280) return ItemRarity.Mitologico;
        if (score > 220) return ItemRarity.Legendario;
        if (score > 160) return ItemRarity.Unico;
        if (score > 110) return ItemRarity.Epico;
        if (score > 70) return ItemRarity.Raro;
        return ItemRarity.Comun;
    }

    private static void ApplyRarityBonuses(CraftedEquipment equipment)
    {
        switch (equipment.Rarity)
        {
            case ItemRarity.Raro:
                equipment.StatBonuses["Fuerza"] = 2;
                equipment.UniqueFlavorText = "Un brillo sutil recubre el filo.";
                break;
            case ItemRarity.Epico:
                equipment.StatBonuses["Fuerza"] = 5;
                equipment.StatBonuses["Agilidad"] = 3;
                equipment.UniqueFlavorText = "El metal zumba con energía latente.";
                break;
            case ItemRarity.Unico:
                equipment.StatBonuses["Fuerza"] = 10;
                equipment.StatBonuses["Agilidad"] = 5;
                equipment.UniqueFlavorText = "Una pieza irrepetible, marcada por la mano de su artesano.";
                break;
            case ItemRarity.Legendario:
                equipment.StatBonuses["Fuerza"] = 25;
                equipment.StatBonuses["Dignidad"] = 10;
                equipment.UniqueFlavorText = "¡Objeto legendario! Su nombre resuena en todo el continente y el creador deja su marca eterna.";
                break;
            case ItemRarity.Mitologico:
                equipment.StatBonuses["Fuerza"] = 40;
                equipment.StatBonuses["Agilidad"] = 20;
                equipment.StatBonuses["Dignidad"] = 20;
                equipment.UniqueFlavorText = "Una reliquia mítica que parece alterar el destino de quien la empuña.";
                break;
            default:
                equipment.UniqueFlavorText = "Un artefacto estándar, cumple su función.";
                break;
        }
    }
}
