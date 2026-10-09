namespace ValleQuebrado.Combat;

public enum PartyRole
{
    TanqueFrontal, SanadorAlquimista, MagoDeApoyo, EspecialistaDano
}

/// <summary>Compañera reclutable para explorar calabozos.</summary>
public sealed class CompanionData
{
    public required string Name { get; init; }
    public PartyRole Role { get; init; }
    public int CombatPower { get; init; }
    public required string SpecialAbility { get; init; }
    public bool IsActiveInParty { get; internal set; }
}

/// <summary>Gestiona las compañeras disponibles y la composición del grupo.</summary>
public static class DungeonPartyManager
{
    private static readonly List<CompanionData> CompanionRoster = new()
    {
        new CompanionData
        {
            Name = "Abigrund", Role = PartyRole.TanqueFrontal, CombatPower = 85,
            SpecialAbility = "Golpe de Gigante (Aturde enemigos)"
        },
        new CompanionData
        {
            Name = "Karrulka", Role = PartyRole.SanadorAlquimista, CombatPower = 60,
            SpecialAbility = "Infusión de Hierbas (Cura al grupo)"
        },
        new CompanionData
        {
            Name = "Pennix", Role = PartyRole.MagoDeApoyo, CombatPower = 70,
            SpecialAbility = "Barrera de Luz Sagrada"
        },
        new CompanionData
        {
            Name = "Emmelith", Role = PartyRole.EspecialistaDano, CombatPower = 65,
            SpecialAbility = "Cristal Explosivo"
        }
    };

    /// <summary>Compañeras disponibles para formar el grupo.</summary>
    public static IReadOnlyList<CompanionData> AvailableCompanions => CompanionRoster.AsReadOnly();

    /// <summary>Compañeras actualmente activas en el grupo.</summary>
    public static IReadOnlyList<CompanionData> ActiveCompanions =>
        CompanionRoster.Where(companion => companion.IsActiveInParty).ToArray();

    /// <summary>Activa o retira una compañera por nombre; devuelve false si no existe.</summary>
    public static bool SetCompanionActive(string name, bool status)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;
        CompanionData? companion = CompanionRoster.FirstOrDefault(candidate =>
            StringComparer.OrdinalIgnoreCase.Equals(candidate.Name, name));
        if (companion is null)
            return false;

        companion.IsActiveInParty = status;
        return true;
    }
}
