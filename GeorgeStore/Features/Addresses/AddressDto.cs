namespace GeorgeStore.Features.Addresses;

public sealed record AddressDto(
    int Id,
    string Alias,
    string Street,
    string Neighborhood,
    string City,
    string State,
    string PostalCode,
    string? ExternalNumber,
    string? InternalNumber,
    string? References,
    bool IsDefault
);

public sealed record AddressCreateDto(
    string Alias,
    string Street,
    string Neighborhood,
    string City,
    string State,
    string PostalCode,
    string? ExternalNumber,
    string? InternalNumber,
    string? References,
    bool IsDefault
);

