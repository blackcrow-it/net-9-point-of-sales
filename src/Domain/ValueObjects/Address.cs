namespace Domain.ValueObjects;

/// <summary>
/// Value object representing a Vietnamese address
/// </summary>
public record Address
{
    public string Street { get; init; }
    public string? Ward { get; init; }
    public string? District { get; init; }
    public string City { get; init; }
    public string? PostalCode { get; init; }

    public Address(string street, string city, string? ward = null, string? district = null, string? postalCode = null)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty", nameof(street));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));

        Street = street;
        Ward = ward;
        District = district;
        City = city;
        PostalCode = postalCode;
    }

    public string GetFullAddress()
    {
        var parts = new List<string> { Street };

        if (!string.IsNullOrWhiteSpace(Ward))
            parts.Add(Ward);

        if (!string.IsNullOrWhiteSpace(District))
            parts.Add(District);

        parts.Add(City);

        if (!string.IsNullOrWhiteSpace(PostalCode))
            parts.Add(PostalCode);

        return string.Join(", ", parts);
    }

    public override string ToString() => GetFullAddress();
}
