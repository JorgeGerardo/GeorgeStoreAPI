using GeorgeStore.Core;

namespace GeorgeStore.Models;

public class Cart : Entity
{
    public User? User { get; set; }
    public required Guid UserId { get; set; }
    public float Total { get; set; }
    public List<Product> Products { get; set; } = [];
}


public static class CartErrorDictionary
{
    public static Error ErrorUnexpected(string tittle, string detail, string code) => new(tittle, detail, code);
}