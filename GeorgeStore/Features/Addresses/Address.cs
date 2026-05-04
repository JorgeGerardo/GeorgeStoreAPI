using GeorgeStore.Common.Core;
using GeorgeStore.Features.Users;

namespace GeorgeStore.Features.Addresses;

public class Address : Entity
{
    public required string Alias { get; set; }
    public User User { get; set; } = default!;
    public required Guid UserId { get; set; }
    public required string Street { get; set; }
    public required string Neighborhood { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string PostalCode { get; set; }
    public required bool IsDefault { get; set; }

    public string? ExternalNumber { get; set; }
    public string? InternalNumber { get; set; }
    public string? References { get; set; }

    public static Address Create(
        Guid userId,
        string alias,
        string street,
        string neighborhood,
        string city,
        string state,
        string postalCode,
        string? externalNumber,
        string? internalNumber,
        string? references,
        bool isDefault)
    {
        return new Address()
        {
            Alias = alias,
            UserId = userId,
            Street = street,
            Neighborhood = neighborhood,
            City = city,
            State = state,
            PostalCode = postalCode,
            ExternalNumber = externalNumber,
            InternalNumber = internalNumber,
            References = references,
            IsDefault = isDefault
        };
    }
}
