using System.Collections.ObjectModel;

namespace ValleQuebrado.Crafting;

public enum EquipmentSlot { Weapon, Armor, Accessory }

/// <summary>Receta de equipo fabricable en una forja.</summary>
public sealed record Recipe(
    string ItemName,
    EquipmentSlot Slot,
    IReadOnlyDictionary<string, int> RequiredMaterials,
    int StatBonus,
    bool RequiresBossTrophy);

/// <summary>Comprueba recetas y fabrica equipo consumiendo los materiales requeridos.</summary>
public static class BlacksmithingManager
{
    private static readonly IReadOnlyList<Recipe> RecipeData = Array.AsReadOnly(new[]
    {
        CreateRecipe("Espada de Sótano Roto", EquipmentSlot.Weapon,
            new Dictionary<string, int>
            {
                ["madera_espectral"] = 5,
                ["mineral_hierro"] = 10
            }, statBonus: 12),
        CreateRecipe("Armadura Épica de Jefe (Fragua del Cráter)", EquipmentSlot.Armor,
            new Dictionary<string, int>
            {
                ["lingote_magma"] = 5,
                ["trofeo_general"] = 1
            }, statBonus: 35, requiresBossTrophy: true)
    });

    /// <summary>Recetas disponibles en la Fragua del Cráter.</summary>
    public static IReadOnlyList<Recipe> AvailableRecipes => RecipeData;

    /// <summary>Indica si el inventario contiene todos los materiales de la receta.</summary>
    public static bool CanCraft(Recipe recipe, IReadOnlyDictionary<string, int> playerInventory)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        ArgumentNullException.ThrowIfNull(playerInventory);
        if (recipe.RequiresBossTrophy &&
            (!recipe.RequiredMaterials.TryGetValue("trofeo_general", out int trophyCount) || trophyCount < 1))
            return false;

        foreach ((string material, int required) in recipe.RequiredMaterials)
        {
            if (required <= 0 || !playerInventory.TryGetValue(material, out int quantity) || quantity < required)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Fabrica una receta y descuenta sus materiales. Devuelve false sin modificar
    /// el inventario si falta algún material o la receta no es válida.
    /// </summary>
    public static bool TryCraft(Recipe recipe, IDictionary<string, int> playerInventory)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        ArgumentNullException.ThrowIfNull(playerInventory);
        var inventorySnapshot = new Dictionary<string, int>(playerInventory);
        if (!CanCraft(recipe, inventorySnapshot))
            return false;

        foreach ((string material, int required) in recipe.RequiredMaterials)
        {
            int remaining = playerInventory[material] - required;
            if (remaining == 0)
                playerInventory.Remove(material);
            else
                playerInventory[material] = remaining;
        }
        return true;
    }

    private static Recipe CreateRecipe(
        string name,
        EquipmentSlot slot,
        Dictionary<string, int> materials,
        int statBonus,
        bool requiresBossTrophy = false) =>
        new(name, slot,
            new ReadOnlyDictionary<string, int>(materials),
            statBonus,
            requiresBossTrophy);
}
