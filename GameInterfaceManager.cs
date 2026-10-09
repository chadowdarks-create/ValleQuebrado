namespace ValleQuebrado.UI;

/// <summary>Elemento del HUD táctil y acción asociada.</summary>
public sealed class HudElement
{
    public HudElement(string elementName, string touchAction, bool isVisibleOnMobile = true)
    {
        if (string.IsNullOrWhiteSpace(elementName))
            throw new ArgumentException("El elemento debe tener un nombre.", nameof(elementName));
        if (string.IsNullOrWhiteSpace(touchAction))
            throw new ArgumentException("El elemento debe tener una acción táctil.", nameof(touchAction));

        ElementName = elementName;
        TouchAction = touchAction;
        IsVisibleOnMobile = isVisibleOnMobile;
    }

    public string ElementName { get; }
    public string TouchAction { get; }
    public bool IsVisibleOnMobile { get; set; }
}

/// <summary>Administra el diseño lógico del HUD móvil principal.</summary>
public static class GameInterfaceManager
{
    private static readonly List<HudElement> HudElements = new()
    {
        new("Barra de Vigor y Salud", "Muestra estado físico actual y necesidad de comer"),
        new("Acceso Rápido de Caldero (Cocina)", "Abre menú rápido de Dungeon Meshi con un toque"),
        new("Mapa del Abismo (150 Pisos)", "Despliega el mapa de estratos, escaleras y ubicación de comandantes"),
        new("Menú de Compañeras (Harem)", "Gestiona el equipo activo para bajar al calabozo"),
        new("Mochila y Forja Overgeared", "Controla inventario de minerales raros y rarezas de equipo")
    };

    public static IReadOnlyList<HudElement> MobileHudLayout => HudElements.AsReadOnly();

    /// <summary>Busca un elemento del HUD por nombre, sin distinguir mayúsculas.</summary>
    public static HudElement? FindElement(string elementName) =>
        string.IsNullOrWhiteSpace(elementName)
            ? null
            : HudElements.FirstOrDefault(element =>
                StringComparer.OrdinalIgnoreCase.Equals(element.ElementName, elementName));

    /// <summary>Cambia la visibilidad móvil de un elemento existente.</summary>
    public static bool SetVisible(string elementName, bool visible)
    {
        HudElement? element = FindElement(elementName);
        if (element is null)
            return false;
        element.IsVisibleOnMobile = visible;
        return true;
    }

    /// <summary>Resuelve una pulsación a su acción; devuelve null si no está visible o no existe.</summary>
    public static string? HandleTouch(string elementName)
    {
        HudElement? element = FindElement(elementName);
        return element is { IsVisibleOnMobile: true } ? element.TouchAction : null;
    }
}
