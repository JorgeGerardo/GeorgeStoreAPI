using GeorgeStore.Common;

namespace GeorgeStore.Features.Addresses;

public class Address : Entity
{
    public required string Alias { get; set; }
    public required Guid UserId { get; set; }
    public required string Street { get; set; }
    public required string Neighborhood { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string PostalCode { get; set; }

    public string? ExternalNumber { get; set; }
    public string? InternalNumber { get; set; }
    public string? References { get; set; }
}
