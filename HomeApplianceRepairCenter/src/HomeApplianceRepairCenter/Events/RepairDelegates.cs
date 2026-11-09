namespace HomeApplianceRepairCenter.Events;

// Delegates (type-safe function pointers) that define the SHAPE of the methods
// allowed to subscribe to the events published by an appliance.

/// <summary>Signature of any method that wants to be notified of a status change.</summary>
public delegate void RepairStatusChangedHandler(object sender, RepairStatusChangedEventArgs e);

/// <summary>Signature of any method that wants to be notified of a delivery delay.</summary>
public delegate void DeliveryDelayedHandler(object sender, DeliveryDelayedEventArgs e);
