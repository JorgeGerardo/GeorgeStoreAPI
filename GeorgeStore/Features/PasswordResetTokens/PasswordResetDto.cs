namespace GeorgeStore.Features.PasswordResetTokens;

public sealed record RecoverPassowrdDto(string Email);
public sealed record RecoverPassowordInfoRequest(string Token, string NewPassowrd);
