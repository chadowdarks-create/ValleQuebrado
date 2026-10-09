namespace ValleQuebrado.Simulation;

/// <summary>Reloj del mundo; comienza el primer día a las 06:00.</summary>
public static class TimeAndScheduleManager
{
    public static int CurrentDay { get; private set; } = 1;
    public static int CurrentHour { get; private set; } = 6;

    /// <summary>Avanza horas del mundo y actualiza cultivos y estaciones.</summary>
    public static void AdvanceTime(int hours)
    {
        if (hours < 0)
            throw new ArgumentOutOfRangeException(nameof(hours), "No se puede retroceder el reloj.");
        if (hours == 0)
            return;

        long totalHours = (long)CurrentHour + hours;
        int daysPassed = checked((int)(totalHours / 24));
        CurrentHour = (int)(totalHours % 24);
        CurrentDay = checked(CurrentDay + daysPassed);

        for (int day = 0; day < daysPassed; day++)
            FarmingAndWeatherManager.PassDay();
        CraftingStationsManager.AdvanceWork(hours);
    }
}

/// <summary>Estamina del jugador y recuperación al descansar en la base.</summary>
public static class StaminaEnergyManager
{
    public const float MaxStamina = 100f;
    private static float _currentStamina = MaxStamina;

    public static float CurrentStamina
    {
        get => _currentStamina;
        set => _currentStamina = Math.Clamp(value, 0f, MaxStamina);
    }

    public static bool ConsumeStamina(float amount)
    {
        if (amount < 0f)
            throw new ArgumentOutOfRangeException(nameof(amount));
        if (CurrentStamina < amount)
            return false;

        CurrentStamina -= amount;
        return true;
    }

    public static void RestAtBase()
    {
        CurrentStamina = MaxStamina;
        TimeAndScheduleManager.AdvanceTime(8);
    }
}

/// <summary>Parcela sembrada que crece una vez por cada día transcurrido.</summary>
public sealed class CropPlot
{
    public required string CropName { get; init; }
    public int DaysToGrow { get; init; }
    public int CurrentDaysGrown { get; internal set; }
    public bool IsReadyToHarvest => CurrentDaysGrown >= DaysToGrow;
}

public static class FarmingAndWeatherManager
{
    private static readonly List<CropPlot> Plots = new();
    public static IReadOnlyList<CropPlot> SurfacePlots => Plots.AsReadOnly();

    public static CropPlot PlantSeed(string cropName, int daysNeeded)
    {
        if (string.IsNullOrWhiteSpace(cropName))
            throw new ArgumentException("La semilla debe tener un nombre.", nameof(cropName));
        if (daysNeeded < 1)
            throw new ArgumentOutOfRangeException(nameof(daysNeeded));

        var plot = new CropPlot { CropName = cropName, DaysToGrow = daysNeeded };
        Plots.Add(plot);
        return plot;
    }

    public static void PassDay()
    {
        foreach (CropPlot plot in Plots)
        {
            if (!plot.IsReadyToHarvest)
                plot.CurrentDaysGrown++;
        }
    }

    public static bool Harvest(CropPlot plot)
    {
        ArgumentNullException.ThrowIfNull(plot);
        return plot.IsReadyToHarvest && Plots.Remove(plot);
    }
}

/// <summary>Estación que procesa un objeto usando horas del reloj del mundo.</summary>
public sealed class WorkStation
{
    public required string StationName { get; init; }
    public string? ActiveCraftingItem { get; internal set; }
    public int ProgressTimeRemaining { get; internal set; }
    public bool IsWorking => ActiveCraftingItem is not null && ProgressTimeRemaining > 0;
}

public static class CraftingStationsManager
{
    private static readonly List<WorkStation> WorkStationList = new()
    {
        new WorkStation { StationName = "Horno de Fundición del Consorcio" }
    };

    public static IReadOnlyList<WorkStation> Stations => WorkStationList.AsReadOnly();
    public static event Action<WorkStation, string>? WorkCompleted;

    public static bool StartWork(string stationName, string item, int timeNeeded)
    {
        if (string.IsNullOrWhiteSpace(stationName) || string.IsNullOrWhiteSpace(item) || timeNeeded < 1)
            return false;

        WorkStation? station = WorkStationList.FirstOrDefault(candidate =>
            StringComparer.OrdinalIgnoreCase.Equals(candidate.StationName, stationName));
        if (station is null || station.IsWorking)
            return false;

        station.ActiveCraftingItem = item;
        station.ProgressTimeRemaining = timeNeeded;
        return true;
    }

    internal static void AdvanceWork(int hours)
    {
        foreach (WorkStation station in WorkStationList)
        {
            if (!station.IsWorking)
                continue;

            station.ProgressTimeRemaining = Math.Max(0, station.ProgressTimeRemaining - hours);
            if (station.ProgressTimeRemaining == 0 && station.ActiveCraftingItem is string completedItem)
            {
                station.ActiveCraftingItem = null;
                WorkCompleted?.Invoke(station, completedItem);
            }
        }
    }
}
