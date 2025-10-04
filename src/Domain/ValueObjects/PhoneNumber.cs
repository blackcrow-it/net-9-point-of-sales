using System.Text.RegularExpressions;

namespace Domain.ValueObjects;

/// <summary>
/// Value object representing a Vietnamese phone number
/// </summary>
public partial record PhoneNumber
{
    public string Value { get; init; }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be empty", nameof(value));

        // Remove spaces, dashes, and parentheses
        var cleaned = value.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

        // Remove +84 country code if present
        if (cleaned.StartsWith("+84"))
            cleaned = "0" + cleaned[3..];
        else if (cleaned.StartsWith("84"))
            cleaned = "0" + cleaned[2..];

        if (!IsValidVietnamesePhoneNumber(cleaned))
            throw new ArgumentException("Invalid Vietnamese phone number format. Must be 10 digits starting with 0", nameof(value));

        Value = cleaned;
    }

    private static bool IsValidVietnamesePhoneNumber(string phoneNumber)
    {
        // Vietnamese phone numbers: 10 digits starting with 0
        // Mobile: 03, 05, 07, 08, 09
        // Landline: 02
        return PhoneRegex().IsMatch(phoneNumber);
    }

    [GeneratedRegex(@"^0[2-9]\d{8}$")]
    private static partial Regex PhoneRegex();

    public string GetFormatted()
    {
        if (Value.Length == 10)
        {
            return $"{Value[..4]} {Value[4..7]} {Value[7..]}";
        }
        return Value;
    }

    public string GetInternationalFormat()
    {
        // Convert to international format (+84)
        return $"+84 {Value[1..4]} {Value[4..7]} {Value[7..]}";
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
}
