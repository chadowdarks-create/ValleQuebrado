namespace ValleQuebrado.Dungeon;

/// <summary>Receta de cocina que se puede preparar durante una expedición.</summary>
public sealed record DungeonRecipe(
    string DishName,
    IReadOnlyDictionary<string, int> Ingredients,
    string CombatBuff,
    int VigorRestored);

/// <summary>Comida preparada, lista para consumirse.</summary>
public sealed record CookedDish(string Name, string CombatBuff, int VigorRestored);

/// <summary>Catálogo y operaciones de cocina para la supervivencia en calabozos.</summary>
public static class DungeonCookingManager
{
    private static readonly IReadOnlyList<DungeonRecipe> Recipes = Array.AsReadOnly(new[]
    {
        Recipe("Estofado de Slime con Especias",
            new Dictionary<string, int> { ["Gelatina pegajosa"] = 2, ["Trigo de Tormenta"] = 1 },
            "Resistencia al ácido y daño de veneno reducido un 20%", 35),
        Recipe("Brocheta de Lobo de Tres Ojos a la Plancha",
            new Dictionary<string, int> { ["Carne de bestia de guerra"] = 1, ["Ceniza sombría"] = 1 },
            "+15% de Velocidad de Movimiento y Agilidad en combate", 50),
        Recipe("Sopa Regenerativa de Huesos del Vacío",
            new Dictionary<string, int> { ["Huesos rotos"] = 3, ["Cristal de magma"] = 1 },
            "Regeneración pasiva de salud por cada 10 segundos en el calabozo", 80)
    });

    public static IReadOnlyList<DungeonRecipe> DungeonMenu => Recipes;

    public static bool CanCook(DungeonRecipe recipe, IReadOnlyDictionary<string, int> playerInventory)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        ArgumentNullException.ThrowIfNull(playerInventory);
        return recipe.Ingredients.All(ingredient =>
            ingredient.Value > 0 && playerInventory.TryGetValue(ingredient.Key, out int quantity) &&
            quantity >= ingredient.Value);
    }

    /// <summary>Prepara un plato y consume sus ingredientes de manera atómica.</summary>
    public static bool TryCook(
        DungeonRecipe recipe,
        IDictionary<string, int> playerInventory,
        out CookedDish? dish)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        ArgumentNullException.ThrowIfNull(playerInventory);
        dish = null;

        var snapshot = new Dictionary<string, int>(playerInventory);
        if (!CanCook(recipe, snapshot))
            return false;

        foreach ((string ingredient, int amount) in recipe.Ingredients)
        {
            int remaining = playerInventory[ingredient] - amount;
            if (remaining == 0)
                playerInventory.Remove(ingredient);
            else
                playerInventory[ingredient] = remaining;
        }

        dish = new CookedDish(recipe.DishName, recipe.CombatBuff, recipe.VigorRestored);
        return true;
    }

    /// <summary>Consume un plato y restaura el Vigor indicado, limitado al máximo.</summary>
    public static void Eat(CookedDish dish, CombatManager combat)
    {
        ArgumentNullException.ThrowIfNull(dish);
        ArgumentNullException.ThrowIfNull(combat);
        combat.RestoreVigor(dish.VigorRestored);
    }

    private static DungeonRecipe Recipe(
        string name, Dictionary<string, int> ingredients, string buff, int vigor) =>
        new(name, new System.Collections.ObjectModel.ReadOnlyDictionary<string, int>(ingredients), buff, vigor);
}

public enum FloorType
{
    Normal, SalaDeComandante, SalaDeGeneral
}

/// <summary>Configuración narrativa y de progresión de un piso del calabozo.</summary>
public sealed record DungeonFloorInfo(
    int FloorNumber,
    FloorType Type,
    string? BossOrCommanderName,
    string RequiredStairDescription);

/// <summary>Determina encuentros y descripción de las escaleras de cada piso.</summary>
public static class DungeonFloorManager
{
    public static DungeonFloorInfo GetFloorData(int floor)
    {
        if (floor < 1)
            throw new ArgumentOutOfRangeException(nameof(floor), "El piso debe ser mayor que cero.");

        if (floor % 15 == 0)
            return new DungeonFloorInfo(floor, FloorType.SalaDeGeneral,
                "General Supremo del Vacío",
                "Unas escaleras monumentales de obsidiana descienden hacia el trono de las almas.");

        if (floor % 5 == 0)
            return new DungeonFloorInfo(floor, FloorType.SalaDeComandante,
                "Comandante de Sección del Consorcio",
                "Una escalera de piedra reforzada conduce a la sala del comandante.");

        return new DungeonFloorInfo(floor, FloorType.Normal, null,
            "Una escalera de piedra desciende al siguiente piso.");
    }
}
