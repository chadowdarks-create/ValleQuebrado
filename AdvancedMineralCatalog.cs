using System.Collections.ObjectModel;

namespace ValleQuebrado.Economy;

public enum MineralTier { Normal, Fantasia, Abismal }

/// <summary>Definición extendida de un mineral, su nivel y uso en el mundo.</summary>
public sealed record MineralData(
    string Id,
    string Name,
    MineralTier Tier,
    string InspirationSource,
    string Utility);

/// <summary>Catálogo de minerales normales, fantásticos y abismales.</summary>
public static class AdvancedMineralCatalog
{
    private static readonly Dictionary<string, MineralData> MineralDataById =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["carbon"] = new("carbon", "Carbón", MineralTier.Normal, "Stardew Valley",
                "Combustible para hornos y fundición."),
            ["cobre_rocoso"] = new("cobre_rocoso", "Cobre Rocoso", MineralTier.Normal, "Stardew Valley",
                "Herramientas iniciales y estructuras básicas."),
            ["hierro_espectral"] = new("hierro_espectral", "Hierro de Almas", MineralTier.Normal, "Stardew Valley",
                "Armas de nivel medio y yunque de hierro."),
            ["oro_brillante"] = new("oro_brillante", "Oro Brillante", MineralTier.Normal, "Stardew Valley",
                "Joyería, mejoras rúnicas y comercio avanzado."),
            ["ceniza_sombría"] = new("ceniza_sombría", "Ceniza Sombría", MineralTier.Fantasia, "Graveyard Keeper",
                "Alquimia oscura, polvos rituales y tintes."),
            ["fragmento_estelar"] = new("fragmento_estelar", "Fragmento Estelar", MineralTier.Fantasia, "Elden Ring",
                "Aleaciones de gravedad para armas espaciales."),
            ["cristal_magma"] = new("cristal_magma", "Cristal de Magma", MineralTier.Fantasia, "Elden Ring",
                "Armaduras térmicas y armas de fuego/magma."),
            ["hueso_petrificado"] = new("hueso_petrificado", "Hueso Petrificado", MineralTier.Fantasia, "Graveyard Keeper",
                "Escudos pesados y mangual de combate."),
            ["obsidiana_vacia"] = new("obsidiana_vacia", "Obsidiana Abismal", MineralTier.Abismal,
                "Elden Ring / End-game", "Forja definitiva contra Qixaroth y el Vacío.")
        };

    public static IReadOnlyDictionary<string, MineralData> AllMinerals { get; } =
        new ReadOnlyDictionary<string, MineralData>(MineralDataById);

    public static bool TryGet(string id, out MineralData? mineral) => MineralDataById.TryGetValue(id, out mineral);
}
