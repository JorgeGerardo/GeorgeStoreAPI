using GeorgeStore.Core;
using GeorgeStore.Models;

namespace GeorgeStore.Data;

public interface IUserRepository
{
    public Task<Result<User>> Login(string userName, string password);
    public Task<Result<User>> Exist(string email);
    public Task<Result> Register(string userName, string email, string password);
}
