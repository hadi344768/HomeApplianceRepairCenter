using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// Air conditioner: the gas recharge price grows with the cooling capacity (BTU), inverter units
/// need a board diagnosis, and a technician may have to visit the customer's home.
/// </summary>
public class AirConditioner : Appliance
{
    private const decimal HourlyRate = 18m;
    private const decimal GasPricePer12KBtu = 35m;       // gas price for each 12,000 BTU of capacity
    private const decimal InverterDiagnosisFee = 20m;
    private const decimal OnSiteVisitFee = 25m;

    private readonly int _coolingCapacityBtu;
    private readonly bool _isInverter;
    private bool _requiresOnSiteVisit;

    public AirConditioner(string serialNumber, string brand, string customerName, string faultDescription,
        DateTime receivedDate, DateTime expectedDeliveryDate, int coolingCapacityBtu, bool isInverter)
        : base(serialNumber, brand, customerName, faultDescription, receivedDate, expectedDeliveryDate)
    {
        if (!ValidationHelper.HasTypePrefix(serialNumber, "AC"))
            throw new ArgumentException("An air conditioner serial number must start with AC-.", nameof(serialNumber));
        if (!ValidationHelper.IsPositive(coolingCapacityBtu))
            throw new ArgumentOutOfRangeException(nameof(coolingCapacityBtu), "Cooling capacity must be positive.");

        _coolingCapacityBtu = coolingCapacityBtu;
        _isInverter = isInverter;
    }

    public override string ApplianceType => "Air Conditioner";

    public int CoolingCapacityBtu => _coolingCapacityBtu;
    public bool IsInverter => _isInverter;

    /// <summary>True when the work was done at the customer's home.</summary>
    public bool RequiresOnSiteVisit
    {
        get => _requiresOnSiteVisit;
        set
        {
            EnsureCaseIsOpen();
            _requiresOnSiteVisit = value;
        }
    }

    /// <summary>labor + parts + gas (by capacity) (+ inverter diagnosis) (+ on-site visit).</summary>
    public override decimal CalculateRepairCost()
    {
        decimal gasCost = _coolingCapacityBtu / 12000m * GasPricePer12KBtu;
        decimal cost = LaborHours * HourlyRate + PartsCost + gasCost;
        if (_isInverter) cost += InverterDiagnosisFee;
        if (_requiresOnSiteVisit) cost += OnSiteVisitFee;
        return Math.Round(cost, 2);
    }

    public override string GetSummary() =>
        base.GetSummary() + $" | {_coolingCapacityBtu} BTU, {(_isInverter ? "inverter" : "standard")}";
}
