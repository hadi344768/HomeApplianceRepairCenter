using System.Globalization;
using HomeApplianceRepairCenter.Enums;
using HomeApplianceRepairCenter.Events;
using HomeApplianceRepairCenter.Interfaces;
using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter.Models;

/// <summary>
/// Abstraction 2/2: abstract base class that implements <see cref="IRepairable"/>.
/// It holds the state and behavior shared by all appliances and leaves the cost calculation
/// (which depends on the appliance type) abstract.
/// Encapsulation: every field is private and exposed only through validated properties.
/// </summary>
public abstract class Appliance : IRepairable
{
    // ---------- private fields (encapsulation) ----------
    private readonly string _serialNumber;      // never changes after creation
    private readonly string _brand;             // never changes after creation
    private readonly DateTime _receivedDate;    // never changes after creation
    private string _customerName;
    private string _faultDescription;
    private DateTime _expectedDeliveryDate;
    private DateTime? _deliveryDate;
    private RepairStatus _status;
    private decimal _laborHours;
    private decimal _partsCost;
    private bool _isTracked;                    // true once the appliance belongs to a repair center

    // ---------- events (delegate-based notifications) ----------
    /// <summary>Raised every time the status of this appliance changes.</summary>
    public event RepairStatusChangedHandler? StatusChanged;

    /// <summary>Raised when the promised delivery date has passed and the appliance is not delivered.</summary>
    public event DeliveryDelayedHandler? DeliveryDelayed;

    protected Appliance(string serialNumber, string brand, string customerName,
        string faultDescription, DateTime receivedDate, DateTime expectedDeliveryDate)
    {
        if (!ValidationHelper.IsValidSerialNumber(serialNumber))
            throw new ArgumentException("Serial number must look like RF-2026-0001.", nameof(serialNumber));
        if (!ValidationHelper.IsValidName(brand))
            throw new ArgumentException("Brand name is not valid.", nameof(brand));
        if (!ValidationHelper.IsValidName(customerName))
            throw new ArgumentException("Customer name is not valid.", nameof(customerName));
        if (!ValidationHelper.IsValidText(faultDescription, 3, 200))
            throw new ArgumentException("Fault description must contain 3 to 200 characters.", nameof(faultDescription));
        if (!ValidationHelper.IsValidDate(receivedDate))
            throw new ArgumentException("Reception date is out of the supported range.", nameof(receivedDate));
        if (!ValidationHelper.IsValidDeliveryDate(receivedDate, expectedDeliveryDate))
            throw new ArgumentException("Expected delivery date cannot be before the reception date.", nameof(expectedDeliveryDate));

        _serialNumber = serialNumber;
        _brand = brand.Trim();
        _customerName = customerName.Trim();
        _faultDescription = faultDescription.Trim();
        _receivedDate = receivedDate.Date;
        _expectedDeliveryDate = expectedDeliveryDate.Date;
        _status = RepairStatus.Received;
    }

    // ---------- read-only after creation ----------
    public string SerialNumber => _serialNumber;
    public string Brand => _brand;
    public DateTime ReceivedDate => _receivedDate;

    // ---------- read-only from the outside (changed only by workflow methods) ----------
    public RepairStatus Status => _status;
    public DateTime? DeliveryDate => _deliveryDate;

    // ---------- editable only while the case is still open ----------
    public string CustomerName
    {
        get => _customerName;
        set
        {
            EnsureCaseIsOpen();
            if (!ValidationHelper.IsValidName(value))
                throw new ArgumentException("Customer name is not valid.", nameof(value));
            _customerName = value.Trim();
        }
    }

    public string FaultDescription
    {
        get => _faultDescription;
        set
        {
            EnsureCaseIsOpen();
            if (!ValidationHelper.IsValidText(value, 3, 200))
                throw new ArgumentException("Fault description must contain 3 to 200 characters.", nameof(value));
            _faultDescription = value.Trim();
        }
    }

    public DateTime ExpectedDeliveryDate
    {
        get => _expectedDeliveryDate;
        set
        {
            EnsureCaseIsOpen();
            if (!ValidationHelper.IsValidDeliveryDate(_receivedDate, value))
                throw new ArgumentException("Expected delivery date cannot be before the reception date.", nameof(value));
            _expectedDeliveryDate = value.Date;
        }
    }

    /// <summary>Technician hours spent on the appliance.</summary>
    public decimal LaborHours
    {
        get => _laborHours;
        set
        {
            EnsureCaseIsOpen();
            if (!ValidationHelper.IsNonNegative(value))
                throw new ArgumentOutOfRangeException(nameof(value), "Labor hours cannot be negative.");
            _laborHours = value;
        }
    }

    /// <summary>Price of the spare parts used in the repair.</summary>
    public decimal PartsCost
    {
        get => _partsCost;
        set
        {
            EnsureCaseIsOpen();
            if (!ValidationHelper.IsNonNegative(value))
                throw new ArgumentOutOfRangeException(nameof(value), "Parts cost cannot be negative.");
            _partsCost = value;
        }
    }

    /// <summary>Human readable type name, defined by each concrete appliance.</summary>
    public abstract string ApplianceType { get; }

    // ---------- the abstract (type-specific) behavior ----------
    /// <summary>Each appliance type applies its own pricing rules.</summary>
    public abstract decimal CalculateRepairCost();

    // ---------- shared workflow (identical for all appliance types) ----------
    public void StartRepair()
    {
        if (_status != RepairStatus.Received)
            throw new InvalidOperationException($"Cannot start repair from status {_status}.");
        ChangeStatus(RepairStatus.UnderRepair);
    }

    public void CompleteRepair()
    {
        if (_status != RepairStatus.UnderRepair)
            throw new InvalidOperationException($"Cannot complete repair from status {_status}.");
        ChangeStatus(RepairStatus.Repaired);
    }

    public void DeliverToCustomer(DateTime deliveryDate)
    {
        if (_status != RepairStatus.Repaired)
            throw new InvalidOperationException($"Cannot deliver an appliance in status {_status}.");
        if (deliveryDate.Date < _receivedDate)
            throw new ArgumentException("Delivery date cannot be before the reception date.", nameof(deliveryDate));

        _deliveryDate = deliveryDate.Date;
        if (_isTracked)
            RepairStatistics.AddRevenue(CalculateRepairCost());   // cost is frozen from now on
        ChangeStatus(RepairStatus.Delivered);
    }

    /// <summary>
    /// Raises <see cref="DeliveryDelayed"/> when the promised date has passed and the appliance
    /// is not yet delivered. Returns true when the event was raised.
    /// </summary>
    public bool CheckDelay(DateTime today)
    {
        if (_status == RepairStatus.Delivered || today.Date <= _expectedDeliveryDate)
            return false;

        int daysLate = (today.Date - _expectedDeliveryDate).Days;
        OnDeliveryDelayed(new DeliveryDelayedEventArgs(_serialNumber, _customerName, _expectedDeliveryDate, daysLate));
        return true;
    }

    /// <summary>Common report line; derived classes extend it with their own details.</summary>
    public virtual string GetSummary() =>
        $"[{_serialNumber}] {ApplianceType,-16} {_brand,-9} {_customerName,-12} " +
        $"{_status,-11} Cost: {FormatMoney(CalculateRepairCost()),11}";

    public override string ToString() => GetSummary();

    /// <summary>
    /// Called by the repair center when the appliance is registered (internal: same assembly only).
    /// From this moment the appliance is counted in <see cref="RepairStatistics"/>.
    /// </summary>
    internal void StartTracking()
    {
        if (_isTracked) return;
        _isTracked = true;
        RepairStatistics.Register(_status);
    }

    // ---------- helpers ----------
    protected static string FormatMoney(decimal amount) =>
        amount.ToString("N2", CultureInfo.InvariantCulture) + " USD";

    /// <summary>Protected so derived classes can lock their own editable properties too.</summary>
    protected void EnsureCaseIsOpen()
    {
        if (_status == RepairStatus.Delivered)
            throw new InvalidOperationException("The appliance was delivered; its data is locked.");
    }

    private void ChangeStatus(RepairStatus newStatus)
    {
        RepairStatus oldStatus = _status;
        _status = newStatus;
        if (_isTracked)
            RepairStatistics.RecordTransition(oldStatus, newStatus);

        OnStatusChanged(new RepairStatusChangedEventArgs(
            _serialNumber, _customerName, oldStatus, newStatus, CalculateRepairCost(), DateTime.Now));
    }

    // Standard .NET pattern: protected virtual "On..." methods raise the events.
    protected virtual void OnStatusChanged(RepairStatusChangedEventArgs e) => StatusChanged?.Invoke(this, e);

    protected virtual void OnDeliveryDelayed(DeliveryDelayedEventArgs e) => DeliveryDelayed?.Invoke(this, e);
}
