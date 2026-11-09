using System.Globalization;
using System.Text.RegularExpressions;

namespace HomeApplianceRepairCenter.Utilities;

/// <summary>
/// Static class (cannot be instantiated) that groups the input-validation rules of the system.
/// Appliance constructors and property setters call these methods before accepting any value.
/// </summary>
public static class ValidationHelper
{
    // Serial format: two-letter type code, year, running number. Example: RF-2026-0001
    // RF = Refrigerator, WM = Washing Machine, AC = Air Conditioner
    private static readonly Regex SerialPattern =
        new(@"^(RF|WM|AC)-\d{4}-\d{4}$", RegexOptions.Compiled);

    // Letters (any language, including Arabic), spaces, apostrophe, dot and hyphen; 2 to 50 chars.
    private static readonly Regex NamePattern =
        new(@"^[\p{L}][\p{L}\s'.\-]{1,49}$", RegexOptions.Compiled);

    private static readonly DateTime MinDate = new(2000, 1, 1);
    private static readonly DateTime MaxDate = new(2100, 12, 31);

    /// <summary>True when the serial number matches the pattern XX-YYYY-NNNN.</summary>
    public static bool IsValidSerialNumber(string? serialNumber) =>
        !string.IsNullOrWhiteSpace(serialNumber) && SerialPattern.IsMatch(serialNumber);

    /// <summary>True when the serial number starts with the given two-letter type code.</summary>
    public static bool HasTypePrefix(string serialNumber, string prefix) =>
        serialNumber.StartsWith(prefix + "-", StringComparison.Ordinal);

    /// <summary>True when the value looks like a person or brand name.</summary>
    public static bool IsValidName(string? name) =>
        !string.IsNullOrWhiteSpace(name) && NamePattern.IsMatch(name.Trim());

    /// <summary>True when the text is not blank and its length is within [minLength, maxLength].</summary>
    public static bool IsValidText(string? text, int minLength, int maxLength) =>
        !string.IsNullOrWhiteSpace(text) &&
        text.Trim().Length >= minLength && text.Trim().Length <= maxLength;

    /// <summary>True when the date lies in a realistic range (years 2000 to 2100).</summary>
    public static bool IsValidDate(DateTime date) => date >= MinDate && date <= MaxDate;

    /// <summary>True when the promised delivery date is valid and not before the reception date.</summary>
    public static bool IsValidDeliveryDate(DateTime receivedDate, DateTime expectedDate) =>
        IsValidDate(expectedDate) && expectedDate.Date >= receivedDate.Date;

    /// <summary>Parses a text in the format yyyy-MM-dd; returns false when it is not a real date.</summary>
    public static bool TryParseDate(string? text, out DateTime date)
    {
        if (DateTime.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out date))
        {
            return IsValidDate(date);
        }

        date = default;
        return false;
    }

    /// <summary>True when the amount is zero or positive (labor hours, parts cost).</summary>
    public static bool IsNonNegative(decimal value) => value >= 0m;

    /// <summary>True when the number is strictly positive (capacity values).</summary>
    public static bool IsPositive(int value) => value > 0;
}
