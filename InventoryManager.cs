/// <summary>Clasificación general de un objeto del inventario.</summary>
public enum ItemCategory
{
    Object,
    Tool,
    Resource
}

/// <summary>Datos de un objeto que se puede guardar en el inventario.</summary>
public sealed record ItemDefinition(
    string Id,
    string Name,
    ItemCategory Category,
    bool IsStackable = true,
    int MaxStack = 99);

/// <summary>Contenido de una casilla de inventario.</summary>
public sealed record InventorySlot(ItemDefinition Item, int Quantity);

/// <summary>
/// Inventario en cuadrícula con una hotbar que referencia casillas de la cuadrícula.
/// Agregar objetos apila primero en pilas existentes y luego ocupa casillas vacías.
/// </summary>
public sealed class InventoryManager
{
    private readonly InventorySlot?[] _slots;
    private readonly int?[] _hotbar;

    /// <summary>Crea el inventario con las dimensiones indicadas.</summary>
    public InventoryManager(int rows = 4, int columns = 8, int hotbarSize = 8)
    {
        if (rows < 1)
            throw new ArgumentOutOfRangeException(nameof(rows));
        if (columns < 1)
            throw new ArgumentOutOfRangeException(nameof(columns));
        if (hotbarSize < 1)
            throw new ArgumentOutOfRangeException(nameof(hotbarSize));

        Rows = rows;
        Columns = columns;
        _slots = new InventorySlot?[checked(rows * columns)];
        _hotbar = new int?[hotbarSize];
    }

    public int Rows { get; }
    public int Columns { get; }
    public int Capacity => _slots.Length;
    public int HotbarSize => _hotbar.Length;

    /// <summary>Vista de solo lectura de las casillas de la cuadrícula.</summary>
    public IReadOnlyList<InventorySlot?> Slots => Array.AsReadOnly(_slots);

    /// <summary>Índices de casilla asignados a cada espacio de la hotbar.</summary>
    public IReadOnlyList<int?> Hotbar => Array.AsReadOnly(_hotbar);

    /// <summary>Cantidad total de un objeto identificado por su ID.</summary>
    public int GetQuantity(string itemId) => _slots
        .Where(slot => slot is not null && IdEquals(slot.Item.Id, itemId))
        .Sum(slot => slot!.Quantity);

    /// <summary>
    /// Agrega la cantidad posible y devuelve cuántas unidades no cupieron.
    /// Un objeto no apilable ocupa una casilla por unidad.
    /// </summary>
    public int Add(ItemDefinition item, int quantity = 1)
    {
        ArgumentNullException.ThrowIfNull(item);
        ValidateItem(item);
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));
        if (quantity == 0)
            return 0;

        int remaining = quantity;
        if (item.IsStackable)
        {
            for (int i = 0; i < _slots.Length && remaining > 0; i++)
            {
                InventorySlot? slot = _slots[i];
                if (slot is null || !IdEquals(slot.Item.Id, item.Id) || slot.Quantity >= item.MaxStack)
                    continue;

                int moved = Math.Min(remaining, item.MaxStack - slot.Quantity);
                _slots[i] = slot with { Quantity = slot.Quantity + moved };
                remaining -= moved;
            }
        }

        for (int i = 0; i < _slots.Length && remaining > 0; i++)
        {
            if (_slots[i] is not null)
                continue;

            int moved = item.IsStackable ? Math.Min(remaining, item.MaxStack) : 1;
            _slots[i] = new InventorySlot(item, moved);
            remaining -= moved;
        }

        return remaining;
    }

    /// <summary>Remueve hasta la cantidad indicada y devuelve cuántas unidades quitó.</summary>
    public int Remove(string itemId, int quantity = 1)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            throw new ArgumentException("El ID del objeto no puede estar vacío.", nameof(itemId));
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        int remaining = quantity;
        for (int i = _slots.Length - 1; i >= 0 && remaining > 0; i--)
        {
            InventorySlot? slot = _slots[i];
            if (slot is null || !IdEquals(slot.Item.Id, itemId))
                continue;

            int removed = Math.Min(remaining, slot.Quantity);
            remaining -= removed;
            _slots[i] = removed == slot.Quantity
                ? null
                : slot with { Quantity = slot.Quantity - removed };
        }

        ClearEmptyHotbarReferences();
        return quantity - remaining;
    }

    /// <summary>Asigna una casilla del inventario a un espacio de hotbar.</summary>
    public bool SetHotbarSlot(int hotbarIndex, int? inventoryIndex)
    {
        if (!IsValidHotbarIndex(hotbarIndex))
            return false;
        if (inventoryIndex.HasValue &&
            (!IsValidInventoryIndex(inventoryIndex.Value) || _slots[inventoryIndex.Value] is null))
            return false;

        _hotbar[hotbarIndex] = inventoryIndex;
        return true;
    }

    /// <summary>Devuelve el objeto asignado al espacio de hotbar, si lo hay.</summary>
    public InventorySlot? GetHotbarItem(int hotbarIndex)
    {
        if (!IsValidHotbarIndex(hotbarIndex) || !_hotbar[hotbarIndex].HasValue)
            return null;
        return _slots[_hotbar[hotbarIndex]!.Value];
    }

    /// <summary>Intercambia el contenido de dos casillas de la cuadrícula.</summary>
    public bool SwapSlots(int firstIndex, int secondIndex)
    {
        if (!IsValidInventoryIndex(firstIndex) || !IsValidInventoryIndex(secondIndex))
            return false;

        (_slots[firstIndex], _slots[secondIndex]) = (_slots[secondIndex], _slots[firstIndex]);
        for (int i = 0; i < _hotbar.Length; i++)
        {
            if (_hotbar[i] == firstIndex)
                _hotbar[i] = secondIndex;
            else if (_hotbar[i] == secondIndex)
                _hotbar[i] = firstIndex;
        }
        return true;
    }

    private static bool IdEquals(string first, string second) =>
        StringComparer.OrdinalIgnoreCase.Equals(first, second);

    private static void ValidateItem(ItemDefinition item)
    {
        if (string.IsNullOrWhiteSpace(item.Id))
            throw new ArgumentException("El objeto debe tener un ID.", nameof(item));
        if (string.IsNullOrWhiteSpace(item.Name))
            throw new ArgumentException("El objeto debe tener un nombre.", nameof(item));
        if (item.MaxStack < 1)
            throw new ArgumentOutOfRangeException(nameof(item), "MaxStack debe ser al menos 1.");
    }

    private bool IsValidInventoryIndex(int index) => index >= 0 && index < _slots.Length;
    private bool IsValidHotbarIndex(int index) => index >= 0 && index < _hotbar.Length;

    private void ClearEmptyHotbarReferences()
    {
        for (int i = 0; i < _hotbar.Length; i++)
        {
            if (_hotbar[i].HasValue && _slots[_hotbar[i]!.Value] is null)
                _hotbar[i] = null;
        }
    }
}
