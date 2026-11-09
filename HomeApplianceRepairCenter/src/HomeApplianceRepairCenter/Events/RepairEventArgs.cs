using HomeApplianceRepairCenter.Enums;

namespace HomeApplianceRepairCenter.Events;

/// <summary>Data sent to subscribers when the status of an appliance changes.</summary>
public class RepairStatusChangedEventArgs : EventArgs
{
    public RepairStatusChangedEventArgs(string serialNumber, string customerName,
        RepairStatus oldStatus, RepairStatus newStatus, decimal currentCost, DateTime changedAt)
    {
        SerialNumber = serialNumber;
        CustomerName = customerName;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        CurrentCost = currentCost;
        ChangedAt = changedAt;
    }

    public string SerialNumber { get; }
    public string CustomerName { get; }
    public RepairStatus OldStatus { get; }
    public RepairStatus NewStatus { get; }
    public decimal CurrentCost { get; }
    public DateTime ChangedAt { get; }
}

/// <summary>Data sent to subscribers when an appliance is not ready by its promised date.</summary>
public class DeliveryDelayedEventArgs : EventArgs
{
    public DeliveryDelayedEventArgs(string serialNumber, string customerName,
        DateTime expectedDeliveryDate, int daysLate)
    {
        SerialNumber = serialNumber;
        CustomerName = customerName;
        ExpectedDeliveryDate = expectedDeliveryDate;
        DaysLate = daysLate;
    }

    public string SerialNumber { get; }
    public string CustomerName { get; }
    public DateTime ExpectedDeliveryDate { get; }
    public int DaysLate { get; }
}
