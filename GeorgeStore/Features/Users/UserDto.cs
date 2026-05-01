namespace GeorgeStore.Features.Users;

public sealed record LoginRequest(string Email, string Password);
public sealed record UserDataDto(string Email, string UserName, DateTime DateRegister);
public sealed record CreateUserRequest(string UserName, string Email, string Password);