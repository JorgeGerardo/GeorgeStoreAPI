namespace GeorgeStore.Features.PasswordRecovery;

public sealed record RecoverPassowrdDto(string Email);
public sealed record RecoverPassowordInfoRequest(string Token, string NewPassowrd);
