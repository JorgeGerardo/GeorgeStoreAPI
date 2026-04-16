using GeorgeStore.Common;

namespace GeorgeStore.Features.Users;

public interface IUserService
{
    public Task<Result<User>> Login(string userName, string password);
    public Task<Result<User>> Exist(string email);
    public Task<Result> Register(string userName, string email, string password);
}
