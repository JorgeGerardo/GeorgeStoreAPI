using GeorgeStore.Common.Shared;
using GeorgeStore.Features.PaymentMethods;

namespace GeorgeStore.Tests.PaymentMethodTests;

public class PaymentMethodTest
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenDataIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = PaymentMethod.Create(
            userId,
            "1234567812345678",
            "VISA",
            12,
            DateTime.UtcNow.Year + 1,
            "John Doe"
        );

        // Assert
        Assert.True(result.IsSuccess);

        PaymentMethod? method = result.Value;

        Assert.Equal(userId, method.UserId);
        Assert.Equal("5678", method.LastDigits);
        Assert.Equal("VISA", method.Brand);
        Assert.False(string.IsNullOrWhiteSpace(method.Token));
    }

    [Fact]
    public void Create_ShouldReturnFailure_When_ExpirationYearIsInvalid()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = PaymentMethod.Create(
            userId,
            "1234567812345678",
            "VISA",
            12,
            2500,
            "John Doe"
        );

        Assert.False(result.IsSuccess);
        Assert.IsType<Error>(result.Error);
        Assert.Equal(PaymentMethodError.InvalidExpYear, result.Error);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public void Create_ShouldReturnFailure_When_Invalid_CardNumberLength()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = PaymentMethod.Create(
            userId,
            "12345678123456781",
            "VISA",
            12,
            2030,
            "John Doe"
        );

        Assert.False(result.IsSuccess);
        Assert.IsType<Error>(result.Error);
        Assert.Equal(PaymentMethodError.InvalidCardNumber, result.Error);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);

    }


}
