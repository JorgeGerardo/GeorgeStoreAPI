namespace GeorgeStore.Data;

public interface ICartService
{
    Task<bool> Add(string ProductId, uint Quantity);
    Task<bool> Remove(string ProductId);
}
