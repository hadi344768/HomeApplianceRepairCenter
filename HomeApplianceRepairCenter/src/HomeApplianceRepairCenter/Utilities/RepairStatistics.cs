using HomeApplianceRepairCenter.Enums;

namespace HomeApplianceRepairCenter.Utilities;

/// <summary>
/// Static class that keeps system-wide figures. The public STATIC PROPERTIES compute a total
/// value (revenue) or a count of appliances by status. The mutating methods are internal, so
/// only the appliances themselves can update the figures, never the outside world.
/// </summary>
public static class RepairStatistics
{
    private static readonly object SyncRoot = new();

    private static readonly Dictionary<RepairStatus, int> Counts =
        Enum.GetValues<RepairStatus>().ToDictionary(status => status, _ => 0);

    private static decimal _totalRevenue;

    /// <summary>Number of appliances ever registered in the system.</summary>
    public static int TotalRegistered
    {
        get { lock (SyncRoot) { return Counts.Values.Sum(); } }
    }

    /// <summary>Appliances that are still in maintenance (received or being repaired).</summary>
    public static int StillInMaintenanceCount =>
        CountByStatus(RepairStatus.Received) + CountByStatus(RepairStatus.UnderRepair);

    /// <summary>Appliances whose repair is finished (waiting for pickup or already delivered).</summary>
    public static int RepairedCount =>
        CountByStatus(RepairStatus.Repaired) + CountByStatus(RepairStatus.Delivered);

    /// <summary>Appliances that are fixed and waiting for the customer to collect them.</summary>
    public static int AwaitingPickupCount => CountByStatus(RepairStatus.Repaired);

    /// <summary>Total money collected from appliances already delivered to customers.</summary>
    public static decimal TotalRevenue
    {
        get { lock (SyncRoot) { return _totalRevenue; } }
    }

    /// <summary>Number of appliances currently in the given status.</summary>
    public static int CountByStatus(RepairStatus status)
    {
        lock (SyncRoot) { return Counts[status]; }
    }

    // ---- updated only by the Appliance class (same assembly) ----

    internal static void Register(RepairStatus currentStatus)
    {
        lock (SyncRoot) { Counts[currentStatus]++; }
    }

    internal static void RecordTransition(RepairStatus from, RepairStatus to)
    {
        lock (SyncRoot)
        {
            Counts[from]--;
            Counts[to]++;
        }
    }

    internal static void AddRevenue(decimal amount)
    {
        lock (SyncRoot) { _totalRevenue += amount; }
    }
}
