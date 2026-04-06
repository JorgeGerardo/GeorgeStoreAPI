using GeorgeStore.Models;
using Microsoft.AspNetCore.Identity;

namespace GeorgeStore.Data;

public interface IUserRepository
{
    public Task<User?> Login(string userName, string password);
    public Task<User?> Exist(string userName);
    public Task<bool> Register(string userName, string email, string password);
}
