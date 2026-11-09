using System.Globalization;
using HomeApplianceRepairCenter.Enums;
using HomeApplianceRepairCenter.Models;
using HomeApplianceRepairCenter.Services;
using HomeApplianceRepairCenter.Utilities;

// Console demo of the home appliance repair center. A fixed "today" keeps the output reproducible.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
Console.OutputEncoding = System.Text.Encoding.UTF8;

DateTime today = new(2026, 11, 8);
var center = new RepairCenter("Damascus Home Appliance Repair Center");

PrintTitle("1) Registering appliances");
var fridge1 = new Refrigerator("RF-2026-0001", "Samsung", "Ahmad", "Not cooling", new DateTime(2026, 10, 20), new DateTime(2026, 10, 27), 350, true);
var fridge2 = new Refrigerator("RF-2026-0002", "LG", "Rima", "Water leaking inside", new DateTime(2026, 11, 3), new DateTime(2026, 11, 12), 280, false);
var washer1 = new WashingMachine("WM-2026-0001", "Bosch", "Samer", "Drum does not spin", new DateTime(2026, 10, 25), new DateTime(2026, 11, 2), 8, true);
var washer2 = new WashingMachine("WM-2026-0002", "Zanussi", "Lina", "Pump noise", new DateTime(2026, 11, 5), new DateTime(2026, 11, 15), 6, false);
var ac1 = new AirConditioner("AC-2026-0001", "Gree", "Khaled", "Gas leakage", new DateTime(2026, 10, 18), new DateTime(2026, 10, 30), 24000, true);
var ac2 = new AirConditioner("AC-2026-0002", "Midea", "Hala", "Remote not responding", new DateTime(2026, 11, 6), new DateTime(2026, 11, 14), 12000, false);

foreach (Appliance a in new Appliance[] { fridge1, fridge2, washer1, washer2, ac1, ac2 })
    center.AddAppliance(a);

PrintTitle("2) Technician reports (labor hours, parts, special work)");
fridge1.LaborHours = 3m;  fridge1.PartsCost = 45m;  fridge1.CompressorReplaced = true;
fridge2.LaborHours = 1.5m; fridge2.PartsCost = 8m;
washer1.LaborHours = 4m;  washer1.PartsCost = 60m;  washer1.DrumReplaced = true;
washer2.LaborHours = 1m;  washer2.PartsCost = 15m;
ac1.LaborHours = 2.5m;    ac1.PartsCost = 30m;      ac1.RequiresOnSiteVisit = true;
ac2.LaborHours = 1m;      ac2.PartsCost = 5m;
center.PrintAllStatuses();

PrintTitle("3) Repair workflow (events are raised automatically)");
fridge1.StartRepair();
fridge1.CompleteRepair();                 // raises StatusChanged -> "Repair COMPLETED"
washer1.StartRepair();
ac1.StartRepair();
ac1.CompleteRepair();
fridge1.DeliverToCustomer(new DateTime(2026, 11, 1));
washer2.StartRepair();

PrintTitle("4) Delay check on " + today.ToString("yyyy-MM-dd"));
int late = center.CheckDelays(today);     // raises DeliveryDelayed for each late appliance
Console.WriteLine($"  Late appliances: {late}");

PrintTitle("5) Current state of every appliance (polymorphic report)");
center.PrintAllStatuses();

PrintTitle("6) Totals");
Console.WriteLine($"  Total accrued cost of all appliances : {center.GetTotalCost():N2} USD");
Console.WriteLine($"  Cost of repaired, waiting for pickup : {center.GetTotalCostByStatus(RepairStatus.Repaired):N2} USD");

PrintTitle("7) Static statistics (RepairStatistics)");
Console.WriteLine($"  Registered appliances : {RepairStatistics.TotalRegistered}");
Console.WriteLine($"  Still in maintenance  : {RepairStatistics.StillInMaintenanceCount}");
Console.WriteLine($"  Repaired (all)        : {RepairStatistics.RepairedCount}");
Console.WriteLine($"  Awaiting pickup       : {RepairStatistics.AwaitingPickupCount}");
Console.WriteLine($"  Revenue collected     : {RepairStatistics.TotalRevenue:N2} USD");

PrintTitle("8) Protection tests (all must be rejected)");
Try("Invalid serial number", () => _ = new Refrigerator("XX-1", "LG", "Omar", "Broken", today, today, 200, true));
Try("Wrong prefix for type ", () => _ = new WashingMachine("RF-2026-0099", "LG", "Omar", "Broken", today, today, 7, true));
Try("Delivery before reception", () => _ = new AirConditioner("AC-2026-0099", "LG", "Omar", "Broken", today, today.AddDays(-1), 9000, false));
Try("Edit a delivered appliance", () => fridge1.PartsCost = 999m);
Try("Deliver before repair", () => washer2.DeliverToCustomer(today));
Try("Negative labor hours", () => fridge2.LaborHours = -2m);
Try("Duplicate serial number", () => center.AddAppliance(
    new Refrigerator("RF-2026-0001", "LG", "Omar", "Broken", today, today, 200, true)));
Console.WriteLine($"  TryParseDate(\"2026-13-40\") -> {ValidationHelper.TryParseDate("2026-13-40", out _)}");
Console.WriteLine($"  TryParseDate(\"2026-11-12\") -> {ValidationHelper.TryParseDate("2026-11-12", out _)}");
Console.WriteLine($"  Registered appliances after the rejected attempts: {RepairStatistics.TotalRegistered} (unchanged)");

PrintTitle("9) Notification log kept by the center");
foreach (string n in center.Notifications) Console.WriteLine("  " + n);

static void PrintTitle(string title)
{
    Console.WriteLine();
    Console.WriteLine($"=== {title} ===");
}

static void Try(string label, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  {label}: NOT REJECTED (unexpected)");
    }
    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
    {
        Console.WriteLine($"  {label}: rejected -> {ex.Message.Split('\n')[0]}");
    }
}
