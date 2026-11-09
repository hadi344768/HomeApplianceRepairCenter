using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// Refrigerator: repair is dominated by refrigerant gas and the compressor.
/// Inheritance: derives from <see cref="Appliance"/>. Polymorphism: overrides the cost rule.
/// </summary>
public class Refrigerator : Appliance
{
    private const decimal HourlyRate = 15m;
    private const decimal GasRefillFee = 30m;       // refrigerant gas is refilled in every repair
    private const decimal CompressorFee = 120m;     // extra work when the compressor is replaced
    private const decimal FreezerCheckFee = 10m;    // extra inspection for models with a freezer

    private readonly int _capacityLiters;           // physical property: never changes
    private readonly bool _hasFreezer;              // physical property: never changes
    private bool _compressorReplaced;

    public Refrigerator(string serialNumber, string brand, string customerName, string faultDescription,
        DateTime receivedDate, DateTime expectedDeliveryDate, int capacityLiters, bool hasFreezer)
        : base(serialNumber, brand, customerName, faultDescription, receivedDate, expectedDeliveryDate)
    {
        if (!ValidationHelper.HasTypePrefix(serialNumber, "RF"))
            throw new ArgumentException("A refrigerator serial number must start with RF-.", nameof(serialNumber));
        if (!ValidationHelper.IsPositive(capacityLiters))
            throw new ArgumentOutOfRangeException(nameof(capacityLiters), "Capacity must be positive.");

        _capacityLiters = capacityLiters;
        _hasFreezer = hasFreezer;
    }

    public override string ApplianceType => "Refrigerator";

    public int CapacityLiters => _capacityLiters;
    public bool HasFreezer => _hasFreezer;

    /// <summary>True when the technician had to replace the compressor.</summary>
    public bool CompressorReplaced
    {
        get => _compressorReplaced;
        set
        {
            EnsureCaseIsOpen();
            _compressorReplaced = value;
        }
    }

    /// <summary>labor + parts + gas refill (+ compressor work) (+ freezer inspection).</summary>
    public override decimal CalculateRepairCost()
    {
        decimal cost = LaborHours * HourlyRate + PartsCost + GasRefillFee;
        if (_compressorReplaced) cost += CompressorFee;
        if (_hasFreezer) cost += FreezerCheckFee;
        return Math.Round(cost, 2);
    }

    public override string GetSummary() =>
        base.GetSummary() + $" | {_capacityLiters} L, freezer: {(_hasFreezer ? "yes" : "no")}";
}
