namespace ValleQuebrado.UI;

/// <summary>Perfil inicial del personaje creado por el jugador.</summary>
public sealed record CharacterProfile(
    string Name,
    string AppearanceStyle,
    string FormerJob,
    string StarterPerk);

/// <summary>Opciones de origen y creación inicial del personaje.</summary>
public static class CharacterCreationManager
{
    private static readonly string[] FormerJobs =
    [
        "Oficinista del Consorcio Yoyo (Bonificación: +10% Eficiencia en recolección)",
        "Mecánico de Vehículos y 3D (Bonificación: +15% Durabilidad de herramientas y forja)",
        "Aventurero Errante (Bonificación: +10% Agilidad y velocidad en calabozos)",
        "Alquimista Callejero (Bonificación: +20% Efecto en recetas de cocina Dungeon Meshi)"
    ];

    public static IReadOnlyList<string> AvailableFormerJobs { get; } = Array.AsReadOnly(FormerJobs);

    /// <summary>Crea el perfil y asigna el origen seleccionado o el primero por defecto.</summary>
    public static CharacterProfile CreatePlayer(string? name, string? style, int jobIndex)
    {
        string selectedJob = jobIndex >= 0 && jobIndex < FormerJobs.Length
            ? FormerJobs[jobIndex]
            : FormerJobs[0];

        return new CharacterProfile(
            string.IsNullOrWhiteSpace(name) ? "Fabian" : name.Trim(),
            string.IsNullOrWhiteSpace(style) ? "Estilo aventurero" : style.Trim(),
            selectedJob,
            "Resistencia al Vacío de Nivel 1");
    }
}
