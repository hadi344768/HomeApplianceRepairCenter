using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// Washing machine: mechanical repair (motor, drum, pump). Front-load models are harder to open,
/// so a surcharge is applied to their cost.
/// </summary>
public class WashingMachine : Appliance
{
    private const decimal HourlyRate = 12m;
    private const decimal DrumReplacementFee = 90m;
    private const decimal FrontLoadSurchargeRate = 0.10m;   // +10 % of (labor + parts)

    private readonly int _loadCapacityKg;
    private readonly bool _isFrontLoad;
    private bool _drumReplaced;

    public WashingMachine(string serialNumber, string brand, string customerName, string faultDescription,
        DateTime receivedDate, DateTime expectedDeliveryDate, int loadCapacityKg, bool isFrontLoad)
        : base(serialNumber, brand, customerName, faultDescription, receivedDate, expectedDeliveryDate)
    {
        if (!ValidationHelper.HasTypePrefix(serialNumber, "WM"))
            throw new ArgumentException("A washing machine serial number must start with WM-.", nameof(serialNumber));
        if (!ValidationHelper.IsPositive(loadCapacityKg))
            throw new ArgumentOutOfRangeException(nameof(loadCapacityKg), "Load capacity must be positive.");

        _loadCapacityKg = loadCapacityKg;
        _isFrontLoad = isFrontLoad;
    }

    public override string ApplianceType => "Washing Machine";

    public int LoadCapacityKg => _loadCapacityKg;
    public bool IsFrontLoad => _isFrontLoad;

    /// <summary>True when the drum had to be replaced.</summary>
    public bool DrumReplaced
    {
        get => _drumReplaced;
        set
        {
            EnsureCaseIsOpen();
            _drumReplaced = value;
        }
    }

    /// <summary>(labor + parts) with a front-load surcharge, plus drum replacement work.</summary>
    public override decimal CalculateRepairCost()
    {
        decimal subtotal = LaborHours * HourlyRate + PartsCost;
        if (_isFrontLoad) subtotal += subtotal * FrontLoadSurchargeRate;
        if (_drumReplaced) subtotal += DrumReplacementFee;
        return Math.Round(subtotal, 2);
    }

    public override string GetSummary() =>
        base.GetSummary() + $" | {_loadCapacityKg} kg, {(_isFrontLoad ? "front" : "top")}-load";
}
