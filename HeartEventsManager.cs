namespace ValleQuebrado.Events;

/// <summary>Evento narrativo desbloqueado al alcanzar cierta amistad.</summary>
public sealed record HeartEvent(int RequiredHearts, string EventTitle, string Description);

/// <summary>Registro de eventos de corazón disponibles por personaje.</summary>
public static class HeartEventsDatabase
{
    private static readonly Dictionary<string, IReadOnlyList<HeartEvent>> EventData =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Abigrund"] = Array.AsReadOnly(new[]
            {
                new HeartEvent(2, "Pociones picantes", "Concurso de beber Poción Dragón en la taberna."),
                new HeartEvent(4, "La carta del clan", "Decides si la animas a quedarse o volver con su familia."),
                new HeartEvent(6, "El calabozo secreto", "Combate cooperativo en un piso oculto por un trofeo propio."),
                new HeartEvent(8, "El hermano de piedra", "Enfrentas o medias con Grumbak, su hermano mayor."),
                new HeartEvent(10, "Corona de huesos", "Propuesta de matrimonio gigante frente al pueblo tras entregar el trofeo.")
            }),
            ["Brunhelda"] = Array.AsReadOnly(new[]
            {
                new HeartEvent(2, "Gerardo habla", "La espada consciente grita un secreto del pueblo en la fragua."),
                new HeartEvent(4, "El dragón cobra", "Decides cómo lidiar con la deuda de apuestas del dragón."),
                new HeartEvent(6, "El clan en la puerta", "Defiendes la Fragua del Cráter de sus hermanos."),
                new HeartEvent(8, "Yunque de la libertad", "Forjas armadura con trofeos de jefe y descubres por qué creó a Gerardo."),
                new HeartEvent(10, "Gerardo, el arma", "Brunhelda te propone matrimonio con un martillo de compromiso.")
            })
        };

    /// <summary>Catálogo de eventos. Los personajes sin registro devuelven una lista vacía.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<HeartEvent>> CharacterEvents => EventData;

    public static IReadOnlyList<HeartEvent> GetEvents(string characterName) =>
        characterName is not null && EventData.TryGetValue(characterName, out IReadOnlyList<HeartEvent>? events)
            ? events
            : Array.Empty<HeartEvent>();
}

/// <summary>Consulta y conserva la finalización de eventos durante la partida.</summary>
public sealed class HeartEventsManager
{
    private readonly HashSet<(string Character, string EventTitle)> _completed = new();

    /// <summary>Eventos cuyo umbral de corazones ya alcanzó el personaje.</summary>
    public IReadOnlyList<HeartEvent> GetAvailableEvents(string characterName, int hearts)
    {
        if (string.IsNullOrWhiteSpace(characterName))
            return Array.Empty<HeartEvent>();
        int currentHearts = Math.Clamp(hearts, 0, 10);
        return HeartEventsDatabase.GetEvents(characterName)
            .Where(item => item.RequiredHearts <= currentHearts)
            .ToArray();
    }

    /// <summary>Marca como completado un evento desbloqueado para ese personaje.</summary>
    public bool CompleteEvent(string characterName, string eventTitle, int hearts)
    {
        if (string.IsNullOrWhiteSpace(characterName) || string.IsNullOrWhiteSpace(eventTitle))
            return false;

        HeartEvent? eventDefinition = HeartEventsDatabase.GetEvents(characterName)
            .FirstOrDefault(item => StringComparer.OrdinalIgnoreCase.Equals(item.EventTitle, eventTitle));
        if (eventDefinition is null || eventDefinition.RequiredHearts > Math.Clamp(hearts, 0, 10))
            return false;

        return _completed.Add((characterName, eventDefinition.EventTitle));
    }

    public bool IsCompleted(string characterName, string eventTitle) =>
        _completed.Contains((characterName, eventTitle));

    /// <summary>Devuelve el registro del personaje con el estado de finalización actual.</summary>
    public IReadOnlyList<HeartEventStatus> GetEventStatuses(string characterName)
    {
        return HeartEventsDatabase.GetEvents(characterName)
            .Select(item => new HeartEventStatus(item,
                _completed.Contains((characterName, item.EventTitle))))
            .ToArray();
    }
}

/// <summary>Evento junto con su estado en la partida actual.</summary>
public sealed record HeartEventStatus(HeartEvent Event, bool IsCompleted);
