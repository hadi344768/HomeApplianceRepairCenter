using HomeApplianceRepairCenter.Enums;

namespace HomeApplianceRepairCenter.Interfaces;

/// <summary>
/// Abstraction 1/2: the operations that EVERY appliance handled by the center must support,
/// regardless of its type (refrigerator, washing machine, air conditioner, ...).
/// </summary>
public interface IRepairable
{
    /// <summary>Unique identifier of the appliance (read-only).</summary>
    string SerialNumber { get; }

    /// <summary>Current position of the appliance in the repair workflow.</summary>
    RepairStatus Status { get; }

    /// <summary>Total repair cost; every appliance type has its own pricing rules.</summary>
    decimal CalculateRepairCost();

    /// <summary>Moves the appliance from Received to UnderRepair.</summary>
    void StartRepair();

    /// <summary>Moves the appliance from UnderRepair to Repaired.</summary>
    void CompleteRepair();

    /// <summary>Hands the appliance back to the customer (Repaired -> Delivered).</summary>
    void DeliverToCustomer(DateTime deliveryDate);

    /// <summary>One-line human readable description used in reports.</summary>
    string GetSummary();
}
