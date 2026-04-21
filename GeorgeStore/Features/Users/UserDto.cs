namespace GeorgeStore.Features.Users;

public record LoginRequest(string Email, string Password);
public record UserDataDto(string Email, string UserName, DateTime DateRegister);
public record CreateUserRequest(string UserName, string Email, string Password);