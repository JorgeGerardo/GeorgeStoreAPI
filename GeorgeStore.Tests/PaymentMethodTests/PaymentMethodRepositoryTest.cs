using GeorgeStore.Features.PaymentMethods;
using GeorgeStore.Features.Users;
using GeorgeStore.Infrastructure.Data;
using GeorgeStore.Tests.Common;
using Moq;

namespace GeorgeStore.Tests.PaymentMethodTests;

public class PaymentMethodRepositoryTest
{
    [Fact]
    public async Task AddTest()
    {
        using var context = ContextHelper.Create();
        var conn = new Mock<IDbConnectionFactory>();


        User user = CreateUser(context);

        PaymentMethodRepository paymentRep = new(context, conn.Object);
        PaymentMethodCreateDto request1 = new("1234123412341234", "Visa", 1, 2030, "J Lopez");
        PaymentMethodCreateDto request2 = new("1234123412341234", "Visa", 1, 2030, "J Lopez", true);

        //Act
        var result = await paymentRep.AddAsync(user.Id, request1);
        Assert.True(result.IsSuccess);
        var result2 = await paymentRep.AddAsync(user.Id, request2);
        Assert.True(result2.IsSuccess);
        Assert.Single(user.PaymentMethods, p => p.IsDefault);
    }

    [Fact]
    public async Task Add_LimitedRegisterReached()
    {
        using var context = ContextHelper.Create();
        var conn = new Mock<IDbConnectionFactory>();


        User user = CreateUser(context);
        for (int i = 0; i < PaymentMethodLimits.MaxRegisterPerUser; i++)
        {
            var newPaymentMethod = new PaymentMethod
            {
                User = user,
                Brand = "Visa",
                Token = Guid.NewGuid().ToString(),
                LastDigits = "1234",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                ExpYear = 2039,
                ExpMonth = 1,
                CardHolderName = "Jorguito Lopez",
                IsDefault = true,
            };
            context.PaymentMethods.Add(newPaymentMethod);
        }
        context.SaveChanges();

        PaymentMethodRepository paymentRep = new(context, conn.Object);
        PaymentMethodCreateDto request = new("1234123412341234", "Visa", 1, 2030, "J Lopez");
        //Act
        var result = await paymentRep.AddAsync(user.Id, request);

        Assert.False(result.IsSuccess);
        Assert.Equal(PaymentMethodError.PaymentMethodLimitReached, result.Error);
    }

    [Fact]
    public async Task RemoveTest()
    {
        using var context = ContextHelper.Create();
        var conn = new Mock<IDbConnectionFactory>();


        User user = CreateUser(context);
        var paymentMethod1 = new PaymentMethod
        {
            User = user,
            Brand = "Visa",
            Token = Guid.NewGuid().ToString(),
            LastDigits = "1234",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpYear = 2039,
            ExpMonth = 1,
            CardHolderName = "Jorguito Lopez",
            IsDefault = true,
        };
        var paymentMethod2 = new PaymentMethod
        {
            User = user,
            Brand = "Visa",
            Token = Guid.NewGuid().ToString(),
            LastDigits = "4321",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpYear = 2039,
            ExpMonth = 1,
            CardHolderName = "Jorguito Lopez",
            IsDefault = true,
        };

        user.PaymentMethods = [ paymentMethod1, paymentMethod2 ];

        context.Update(user);
        context.SaveChanges();
        PaymentMethodRepository paymentRep = new(context, conn.Object);

        //Act
        var result = await paymentRep.RemoveAsync(user.Id, paymentMethod2.Id);
        Assert.True(result.IsSuccess);
        Assert.Single(user.PaymentMethods);
        var onlyPaymentM = user.PaymentMethods.FirstOrDefault();
        Assert.NotNull(onlyPaymentM);
        Assert.Equal(paymentMethod1, onlyPaymentM);


    }
    [Fact]
    public async Task Remove_Failure_Notfound()
    {
        using var context = ContextHelper.Create();
        var conn = new Mock<IDbConnectionFactory>();


        User user = CreateUser(context);
        var paymentMethod1 = new PaymentMethod
        {
            User = user,
            Brand = "Visa",
            Token = Guid.NewGuid().ToString(),
            LastDigits = "1234",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpYear = 2039,
            ExpMonth = 1,
            CardHolderName = "Jorguito Lopez",
            IsDefault = true,
        };

        user.PaymentMethods = [ paymentMethod1 ];

        context.Update(user);
        context.SaveChanges();
        PaymentMethodRepository paymentRep = new(context, conn.Object);

        //Act
        var result = await paymentRep.RemoveAsync(user.Id, paymentMethod1.Id);
        Assert.True(result.IsSuccess);
        Assert.Empty(user.PaymentMethods);
        var resultInvalidExpected = await paymentRep.RemoveAsync(user.Id, paymentMethod1.Id);

        Assert.False(resultInvalidExpected.IsSuccess);
        Assert.Equal(PaymentMethodError.NotFound, resultInvalidExpected.Error);

    }


    [Fact]
    public async Task GetByIdTest()
    {
        using var context = ContextHelper.Create();
        var conn = new Mock<IDbConnectionFactory>();


        User user = CreateUser(context);
        var paymentMethod1 = new PaymentMethod
        {
            User = user,
            Brand = "Visa",
            Token = Guid.NewGuid().ToString(),
            LastDigits = "1234",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpYear = 2039,
            ExpMonth = 1,
            CardHolderName = "Jorguito Lopez",
            IsDefault = true,
        };
        var paymentMethod2 = new PaymentMethod
        {
            User = user,
            Brand = "Visa",
            Token = Guid.NewGuid().ToString(),
            LastDigits = "3323",
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            ExpYear = 2033,
            ExpMonth = 3,
            CardHolderName = "Tania Gomez",
            IsDefault = false,
        };
        var paymentMethod3 = new PaymentMethod
        {
            User = user,
            Brand = "Mastercard",
            Token = Guid.NewGuid().ToString(),
            LastDigits = "3323",
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            ExpYear = 2035,
            ExpMonth = 3,
            CardHolderName = "Kenia Ruiz",
            IsDefault = false,
        };

        user.PaymentMethods = [paymentMethod1, paymentMethod2, paymentMethod3];
        context.Update(user);
        context.SaveChanges();


        PaymentMethodRepository paymentRep = new(context, conn.Object);

        //Act
        var result = await paymentRep.GetByIdAsync(user.Id, paymentMethod3.Id);

        Assert.True(result.IsSuccess);
        var paymentMSearched = result.Value;
        Assert.NotNull(paymentMSearched);
        Assert.Equal(paymentMethod3.Token, paymentMSearched.Token);

    }


    [Fact]
    public async Task GetById_NotFound()
    {
        using var context = ContextHelper.Create();
        var conn = new Mock<IDbConnectionFactory>();

        User user = CreateUser(context);

        User user2 = new("Carlita", "carlita@gmail.com");
        var paymentMethod1 = new PaymentMethod
        {
            User = user,
            Brand = "Visa",
            Token = Guid.NewGuid().ToString(),
            LastDigits = "3433",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpYear = 2039,
            ExpMonth = 1,
            CardHolderName = "Carlita Lopez",
            IsDefault = true,
        };
        user2.PaymentMethods = [paymentMethod1];

        context.Add(user2);
        context.SaveChanges();



        PaymentMethodRepository paymentRep = new(context, conn.Object);

        //Act
        var result = await paymentRep.GetByIdAsync(user.Id, paymentMethod1.Id);
        Assert.False(result.IsSuccess);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public async Task SetDefaultMultipleTimesTest()
    {
        using var context = ContextHelper.Create();
        var conn = new Mock<IDbConnectionFactory>();


        User user = CreateUser(context);
        var paymentMethod1 = new PaymentMethod
        {
            User = user,
            Brand = "Visa",
            Token = Guid.NewGuid().ToString(),
            LastDigits = "1234",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpYear = 2039,
            ExpMonth = 1,
            CardHolderName = "Jorguito Lopez",
            IsDefault = true,
        };
        var paymentMethod2 = new PaymentMethod
        {
            User = user,
            Brand = "Visa",
            Token = Guid.NewGuid().ToString(),
            LastDigits = "3323",
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            ExpYear = 2033,
            ExpMonth = 3,
            CardHolderName = "Tania Gomez",
            IsDefault = false,
        };
        var paymentMethod3 = new PaymentMethod
        {
            User = user,
            Brand = "Mastercard",
            Token = Guid.NewGuid().ToString(),
            LastDigits = "3438",
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            ExpYear = 2035,
            ExpMonth = 3,
            CardHolderName = "Kenia Ruiz",
            IsDefault = true,
        };

        user.PaymentMethods = [paymentMethod1, paymentMethod2, paymentMethod3];
        context.Update(user);
        context.SaveChanges();


        PaymentMethodRepository paymentRep = new(context, conn.Object);

        //Act
        var result = await paymentRep.GetByIdAsync(user.Id, paymentMethod3.Id);
        Assert.True(result.IsSuccess);
        var currentDefaultPaymentM = result.Value;
        Assert.NotNull(currentDefaultPaymentM);
        Assert.Equal(paymentMethod3.Id, currentDefaultPaymentM.Id);
    }


    [Fact]
    public async Task SetAsDefault()
    {
        using var context = ContextHelper.Create();
        var conn = new Mock<IDbConnectionFactory>();


        User user = CreateUser(context);
        var paymentMethod1 = new PaymentMethod
        {
            User = user,
            Brand = "Visa",
            Token = Guid.NewGuid().ToString(),
            LastDigits = "1234",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpYear = 2039,
            ExpMonth = 1,
            CardHolderName = "Jorguito Lopez",
            IsDefault = true,
        };
        var paymentMethod2 = new PaymentMethod
        {
            User = user,
            Brand = "Visa",
            Token = Guid.NewGuid().ToString(),
            LastDigits = "3323",
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            ExpYear = 2033,
            ExpMonth = 3,
            CardHolderName = "Tania Gomez",
            IsDefault = true,
        };
        var paymentMethod3 = new PaymentMethod
        {
            User = user,
            Brand = "Mastercard",
            Token = Guid.NewGuid().ToString(),
            LastDigits = "3438",
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            ExpYear = 2035,
            ExpMonth = 3,
            CardHolderName = "Kenia Ruiz",
            IsDefault = false,
        };

        user.PaymentMethods = [paymentMethod1, paymentMethod2, paymentMethod3];
        context.Update(user);
        context.SaveChanges();


        PaymentMethodRepository paymentRep = new(context, conn.Object);

        //Act
        var resultPmSearch = await paymentRep.GetByIdAsync(user.Id, paymentMethod3.Id);
        Assert.True(resultPmSearch.IsSuccess);
        var paymentMethodNotDefault = resultPmSearch.Value;
        Assert.NotNull(paymentMethodNotDefault);
        Assert.False(paymentMethodNotDefault.IsDefault);

        var result = await paymentRep.SetAsDefaultAsync(user.Id, paymentMethod3.Id);
        Assert.True(result.IsSuccess);
        Assert.Single(user.PaymentMethods, p => p.IsDefault);
    }


    [Fact]
    public async Task SetAsDefault_NotFound()
    {
        using var context = ContextHelper.Create();
        var conn = new Mock<IDbConnectionFactory>();


        User userWithoutPM = CreateUser(context);
        User userWithPM = new("Carlita", "carlita@gmail.com");
        var paymentMethod1 = new PaymentMethod
        {
            User = userWithoutPM,
            Brand = "Visa",
            Token = Guid.NewGuid().ToString(),
            LastDigits = "3433",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpYear = 2039,
            ExpMonth = 1,
            CardHolderName = "Carlita Lopez",
            IsDefault = true,
        };
        userWithPM.PaymentMethods = [paymentMethod1];

        context.Update(userWithPM);
        context.SaveChanges();


        PaymentMethodRepository paymentRep = new(context, conn.Object);

        //Act
        var result = await paymentRep.SetAsDefaultAsync(userWithoutPM.Id, paymentMethod1.Id);
        Assert.False(result.IsSuccess);
        Assert.Equal(PaymentMethodError.NotFound, result.Error);
    }

    //Common arranges
    private static User CreateUser(GeorgeStoreContext context)
    {
        User user = new("Jorguito", "jorguito@gmail.com");
        context.Add(user);
        context.SaveChanges();
        return user;
    }

}
