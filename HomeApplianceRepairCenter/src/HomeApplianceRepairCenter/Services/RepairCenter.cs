using HomeApplianceRepairCenter.Enums;
using HomeApplianceRepairCenter.Events;
using HomeApplianceRepairCenter.Models;

namespace HomeApplianceRepairCenter.Services;

/// <summary>
/// Main class of the system. It owns a list of <see cref="Appliance"/> references that actually
/// hold Refrigerator, WashingMachine and AirConditioner objects, and processes them through the
/// base type only (polymorphism). It also SUBSCRIBES to the events of every appliance it registers.
/// </summary>
public class RepairCenter
{
    private readonly List<Appliance> _appliances = new();
    private readonly List<string> _notifications = new();

    public RepairCenter(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Center name is required.", nameof(name));
        Name = name.Trim();
    }

    public string Name { get; }

    /// <summary>Read-only view: callers cannot add or remove items behind the center's back.</summary>
    public IReadOnlyList<Appliance> Appliances => _appliances.AsReadOnly();

    /// <summary>Every notification received through the events, in order.</summary>
    public IReadOnlyList<string> Notifications => _notifications.AsReadOnly();

    /// <summary>Registers an appliance, subscribes the center to its events and starts counting it.</summary>
    public void AddAppliance(Appliance appliance)
    {
        ArgumentNullException.ThrowIfNull(appliance);
        if (FindBySerial(appliance.SerialNumber) is not null)
            throw new InvalidOperationException($"An appliance with serial {appliance.SerialNumber} already exists.");

        appliance.StatusChanged += HandleStatusChanged;       // subscribe: delegate instance is created here
        appliance.DeliveryDelayed += HandleDeliveryDelayed;
        _appliances.Add(appliance);
        appliance.StartTracking();                            // now counted in RepairStatistics
    }

    public Appliance? FindBySerial(string serialNumber) =>
        _appliances.FirstOrDefault(a => a.SerialNumber == serialNumber);

    public IEnumerable<Appliance> GetByStatus(RepairStatus status) =>
        _appliances.Where(a => a.Status == status);

    /// <summary>Polymorphism: each element runs ITS OWN CalculateRepairCost().</summary>
    public decimal GetTotalCost() => _appliances.Sum(a => a.CalculateRepairCost());

    public decimal GetTotalCostByStatus(RepairStatus status) =>
        GetByStatus(status).Sum(a => a.CalculateRepairCost());

    /// <summary>Polymorphism: each element runs ITS OWN GetSummary().</summary>
    public void PrintAllStatuses()
    {
        foreach (Appliance appliance in _appliances)
            Console.WriteLine("  " + appliance.GetSummary());
    }

    /// <summary>Asks every appliance to check its promised date; returns how many are late.</summary>
    public int CheckDelays(DateTime today) => _appliances.Count(a => a.CheckDelay(today));

    // ---------- event handlers (their signature matches the delegates) ----------
    private void HandleStatusChanged(object sender, RepairStatusChangedEventArgs e)
    {
        string message = e.NewStatus switch
        {
            RepairStatus.Repaired =>
                $"Repair COMPLETED for {e.SerialNumber}. Please call {e.CustomerName}. Amount due: {e.CurrentCost:N2} USD.",
            RepairStatus.Delivered =>
                $"{e.SerialNumber} delivered to {e.CustomerName}. Final cost: {e.CurrentCost:N2} USD.",
            _ => $"{e.SerialNumber}: {e.OldStatus} -> {e.NewStatus}."
        };
        Publish("STATUS", message);
    }

    private void HandleDeliveryDelayed(object sender, DeliveryDelayedEventArgs e) =>
        Publish("DELAY ", $"{e.SerialNumber} of {e.CustomerName} is {e.DaysLate} day(s) late " +
                          $"(promised {e.ExpectedDeliveryDate:yyyy-MM-dd}).");

    private void Publish(string kind, string message)
    {
        string line = $"[{kind}] {message}";
        _notifications.Add(line);

        ConsoleColor previous = Console.ForegroundColor;
        Console.ForegroundColor = kind == "DELAY " ? ConsoleColor.Yellow : ConsoleColor.Cyan;
        Console.WriteLine("  >> " + line);
        Console.ForegroundColor = previous;
    }
}
