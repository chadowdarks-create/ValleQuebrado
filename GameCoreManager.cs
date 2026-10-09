using System.Text.Json;

namespace ValleQuebrado.Core;

public enum GameState
{
    SuperficieGranja,
    CalabozoExploracion,
    CampamentoCocinaMeshi,
    CombatePorTurnos,
    JefeSupremoEncuentro
}

/// <summary>Estado global básico y progreso por los pisos del calabozo.</summary>
public static class GameLoopManager
{
    public const int MaximumFloor = 150;

    public static GameState CurrentState { get; private set; } = GameState.SuperficieGranja;
    public static int CurrentFloor { get; private set; } = 1;

    public static event Action<GameState, GameState>? StateChanged;

    public static void ChangeState(GameState newState)
    {
        if (!Enum.IsDefined(newState))
            throw new ArgumentOutOfRangeException(nameof(newState));
        if (CurrentState == newState)
            return;

        GameState previous = CurrentState;
        CurrentState = newState;
        StateChanged?.Invoke(previous, newState);
    }

    /// <summary>Desciende un piso, sin superar el límite del abismo.</summary>
    public static bool DescendFloor()
    {
        if (CurrentFloor >= MaximumFloor)
            return false;
        CurrentFloor++;
        return true;
    }

    public static bool AscendFloor()
    {
        if (CurrentFloor <= 1)
            return false;
        CurrentFloor--;
        return true;
    }

    public static void SetFloor(int floor)
    {
        if (floor < 1 || floor > MaximumFloor)
            throw new ArgumentOutOfRangeException(nameof(floor), $"El piso debe estar entre 1 y {MaximumFloor}.");
        CurrentFloor = floor;
    }
}

/// <summary>Estado del menú actualmente abierto en la interfaz táctil.</summary>
public static class UIManager
{
    public const string MainHudMenu = "HUD_Principal";
    private static string _activeMenu = MainHudMenu;

    public static event Action<string, string>? ActiveMenuChanged;

    public static void OpenMenu(string menuName)
    {
        if (string.IsNullOrWhiteSpace(menuName))
            throw new ArgumentException("El menú debe tener un nombre.", nameof(menuName));
        if (StringComparer.Ordinal.Equals(_activeMenu, menuName))
            return;

        string previous = _activeMenu;
        _activeMenu = menuName;
        ActiveMenuChanged?.Invoke(previous, _activeMenu);
    }

    public static string GetActiveMenu() => _activeMenu;

    public static void CloseMenu() => OpenMenu(MainHudMenu);
}

/// <summary>Datos persistentes de una partida local.</summary>
[Serializable]
public sealed class GameSaveData
{
    public string PlayerName { get; set; } = "Fabian";
    public int FloorReached { get; set; } = 1;
    public int GoldOrSouls { get; set; }
    public List<string> InventoryItems { get; set; } = new();
    public List<string> UnlockedRecipes { get; set; } = new();
}

/// <summary>Guarda y carga partidas en formato JSON en el almacenamiento local.</summary>
public static class SaveLoadManager
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static void SaveGame(GameSaveData data, string filePath)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidatePath(filePath);
        if (data.FloorReached < 1 || data.FloorReached > GameLoopManager.MaximumFloor)
            throw new ArgumentOutOfRangeException(nameof(data), "El piso guardado está fuera del rango del calabozo.");

        string fullPath = Path.GetFullPath(filePath);
        string? directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        string json = JsonSerializer.Serialize(data, JsonOptions);
        File.WriteAllText(fullPath, json);
    }

    public static GameSaveData LoadGame(string filePath)
    {
        ValidatePath(filePath);
        string json = File.ReadAllText(filePath);
        GameSaveData data = JsonSerializer.Deserialize<GameSaveData>(json, JsonOptions)
            ?? throw new InvalidDataException("El archivo de guardado no contiene una partida válida.");

        data.InventoryItems ??= new List<string>();
        data.UnlockedRecipes ??= new List<string>();
        if (data.FloorReached < 1 || data.FloorReached > GameLoopManager.MaximumFloor)
            throw new InvalidDataException("El piso guardado está fuera del rango del calabozo.");
        return data;
    }

    public static bool TryLoadGame(string filePath, out GameSaveData? data)
    {
        data = null;
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return false;
        try
        {
            data = LoadGame(filePath);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }

    private static void ValidatePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("La ruta del archivo no puede estar vacía.", nameof(filePath));
    }
}
