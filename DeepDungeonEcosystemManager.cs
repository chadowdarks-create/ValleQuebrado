namespace ValleQuebrado.Dungeon;

/// <summary>Rango de pisos con un entorno y peligro ambiental propios.</summary>
public sealed record AbyssStratum(
    int MinFloor,
    int MaxFloor,
    string StratumName,
    string EnvironmentalHazard);

/// <summary>Define los 150 pisos del abismo, sus estratos y pisos de jefes.</summary>
public static class DeepDungeonEcosystemManager
{
    public const int TotalDungeonFloors = 150;

    private static readonly IReadOnlyList<AbyssStratum> StratumData = Array.AsReadOnly(new[]
    {
        new AbyssStratum(1, 45, "Los Estratos Superiores (Sótano y Alcantarillas)",
            "Humedad y descomposición ácida"),
        new AbyssStratum(46, 105, "Los Estratos Medios (Túneles de Magma y Minas)",
            "Calor geotérmico y gases tóxicos"),
        new AbyssStratum(106, 150, "Los Estratos Abismales (Fortaleza del Vacío)",
            "Presión de almas y distorsión espacial")
    });

    public static IReadOnlyList<AbyssStratum> Strata => StratumData;

    /// <summary>Obtiene el estrato del piso; rechaza valores fuera del abismo.</summary>
    public static AbyssStratum GetStratumByFloor(int floor)
    {
        if (floor < 1 || floor > TotalDungeonFloors)
            throw new ArgumentOutOfRangeException(nameof(floor), $"El piso debe estar entre 1 y {TotalDungeonFloors}.");

        return StratumData.First(stratum => floor >= stratum.MinFloor && floor <= stratum.MaxFloor);
    }

    public static bool IsGeneralFloor(int floor) =>
        floor is > 0 and <= TotalDungeonFloors && floor % 15 == 0;

    public static bool IsCommanderFloor(int floor) =>
        floor is > 0 and <= TotalDungeonFloors && floor % 5 == 0 && !IsGeneralFloor(floor);
}
