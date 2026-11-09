namespace HomeApplianceRepairCenter.Enums;

/// <summary>
/// Life cycle of an appliance inside the repair center.
/// Received -> UnderRepair -> Repaired -> Delivered
/// </summary>
public enum RepairStatus
{
    Received = 0,     // appliance handed in by the customer, not yet touched
    UnderRepair = 1,  // a technician is working on it
    Repaired = 2,     // repair finished, waiting for the customer to pick it up
    Delivered = 3     // returned to the customer, case closed
}
