using GeorgeStore.Core;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Models;

public class Cart : Entity
{
    public User? User { get; set; }
    public required Guid UserId { get; set; }
    public float Total { get; set; }
    public List<Product> Products { get; set; } = new();
}


public static class CartErrorDictionary
{
    public static Error ErrorUnexpected(string tittle, string detail) => new Error(tittle, detail);
}