namespace ValleQuebrado.Gameplay;

/// <summary>Evento de afinidad que puede desbloquear una pasiva.</summary>
public sealed record HeartEvent(
    string CompanionName,
    int RequiredHearts,
    string EventTitle,
    string UnlockedPassiveBonus);

/// <summary>Eventos y pasivas asociadas a la afinidad con compañeras y jefes.</summary>
public static class HeartEventsManager
{
    private static readonly IReadOnlyList<HeartEvent> EventData = Array.AsReadOnly(new[]
    {
        new HeartEvent("Vesperia", 4, "El Secreto del Velo Espectral",
            "+15% de probabilidad de esquivar ataques en los estratos abismales"),
        new HeartEvent("Malakor", 8, "Forja bajo el Calor del Magma",
            "Desbloquea recetas de armas de rareza Épica sin penalización")
    });

    public static IReadOnlyList<HeartEvent> Events => EventData;

    public static bool CanTriggerEvent(string companion, int currentHearts) =>
        EventData.Any(item => StringComparer.OrdinalIgnoreCase.Equals(item.CompanionName, companion) &&
                              currentHearts >= item.RequiredHearts);

    public static IReadOnlyList<HeartEvent> GetUnlockedEvents(string companion, int currentHearts) =>
        EventData.Where(item => StringComparer.OrdinalIgnoreCase.Equals(item.CompanionName, companion) &&
                                currentHearts >= item.RequiredHearts).ToArray();
}

public enum TacticalActionType
{
    CargaFrontal,
    RetiradaTacticaYCocina,
    OfensivaDeCompañeras,
    UsoDeItemSupervivencia
}

/// <summary>Resuelve la respuesta narrativa de una acción táctica según el Vigor.</summary>
public static class RealTimeTacticalCombatManager
{
    public static string ExecuteTacticalAction(TacticalActionType action, int currentVigor)
    {
        if (currentVigor <= 10 && action != TacticalActionType.UsoDeItemSupervivencia)
            return "¡Alerta! Vigor crítico. El grupo se fatiga; se requiere consumir un plato de Dungeon Meshi inmediatamente.";

        return action switch
        {
            TacticalActionType.CargaFrontal => "El grupo ejecuta una embestida coordinada rompiendo la formación del enemigo raso.",
            TacticalActionType.RetiradaTacticaYCocina => "Las compañeras cubren la retaguardia mientras el equipo enciende una hoguera rápida.",
            TacticalActionType.OfensivaDeCompañeras => "Las habilidades de las compañeras se sincronizan, infligiendo daño masivo al Comandante de Sección.",
            TacticalActionType.UsoDeItemSupervivencia => "Se consumen raciones de monstruo para restaurar el Vigor del grupo en plena marcha.",
            _ => "El grupo mantiene la formación explorando el calabozo."
        };
    }
}

/// <summary>Traduce las coordenadas táctiles de pantalla a acciones de interfaz.</summary>
public static class MobileTouchInputManager
{
    public static string HandleTouchInput(float touchX, float touchY, string currentScreen)
    {
        if (string.IsNullOrWhiteSpace(currentScreen))
            return "Navegacion_Libre";

        if (StringComparer.OrdinalIgnoreCase.Equals(currentScreen, "HUD_Principal"))
        {
            if (touchX < 100f && touchY < 100f)
                return "Abrir_Inventario_Overgeared";
            if (touchX >= 100f && touchY < 100f)
                return "Abrir_Caldero_DungeonMeshi";
        }
        else if (StringComparer.OrdinalIgnoreCase.Equals(currentScreen, "Calabozo") && touchY > 800f)
        {
            return "Accionar_Escalon_Descenso";
        }

        return "Navegacion_Libre";
    }
}
