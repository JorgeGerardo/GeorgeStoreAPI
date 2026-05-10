using GeorgeStore.Common.Shared;
using GeorgeStore.Features.PaymentMethods;
using GeorgeStore.Features.Users;
using GeorgeStore.Tests.Common;
using GeorgeStore.Tests.Factories;

namespace GeorgeStore.Tests.PaymentMethods;

public class PaymentMethodRepositoryTests
{
    [Fact]
    public async Task AddTest()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);

        PaymentMethodRepository paymentRep = PaymentMethodFactory.CreatePaymentMethodRepository(context);
        PaymentMethodCreateDto request1 = new("1234123412341234", "Visa", 1, 2030, "J Lopez");
        PaymentMethodCreateDto request2 = new("1234123412341234", "Visa", 1, 2030, "J Lopez", true);

        //Act
        Result result1 = await paymentRep.AddAsync(user.Id, request1);
        Assert.True(result1.IsSuccess);
        Result result2 = await paymentRep.AddAsync(user.Id, request2);
        Assert.True(result2.IsSuccess);
        Assert.Single(user.PaymentMethods, p => p.IsDefault);
        Assert.Equal(2, user.PaymentMethods.Count);
    }

    [Fact]
    public async Task Add_LimitedRegisterReached()
    {
        using var context = ContextHelper.Create();

        User user = ContextHelper.CreateUser(context);
        for (int i = 0; i < PaymentMethodLimits.MaxRegisterPerUser; i++)
            PaymentMethodFactory.CreateRandomPaymentMethod(context, user);

        PaymentMethodRepository paymentRep = PaymentMethodFactory.CreatePaymentMethodRepository(context);
        PaymentMethodCreateDto request = new("1234123412341234", "Visa", 1, 2030, "J Lopez");
        //Act
        Result result = await paymentRep.AddAsync(user.Id, request);
        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PaymentMethodError.PaymentMethodLimitReached, result.Error);
    }

    [Fact]
    public async Task RemoveTest()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        PaymentMethod? paymentMethod1 = PaymentMethodFactory.CreateRandomPaymentMethod(context, user, IsDefault: true);
        PaymentMethod? paymentMethod2 = PaymentMethodFactory.CreateRandomPaymentMethod(context, user, IsDefault: true);
        PaymentMethodRepository paymentRep = PaymentMethodFactory.CreatePaymentMethodRepository(context);

        //Act
        Result result = await paymentRep.RemoveAsync(user.Id, paymentMethod2.Id);
        
        //Assert
        Assert.True(result.IsSuccess);
        Assert.Single(user.PaymentMethods);
        PaymentMethod? onlyPaymentM = user.PaymentMethods.FirstOrDefault();
        Assert.NotNull(onlyPaymentM);
        Assert.Equal(paymentMethod1, onlyPaymentM);
    }


    [Fact]
    public async Task Remove_Failure_Notfound()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        PaymentMethod newPaymentM = PaymentMethodFactory.CreateRandomPaymentMethod(context, user, IsDefault: true);
        PaymentMethodRepository paymentRep = PaymentMethodFactory.CreatePaymentMethodRepository(context);

        //Act
        var result = await paymentRep.RemoveAsync(user.Id, newPaymentM.Id);
        Assert.True(result.IsSuccess);
        Assert.Empty(user.PaymentMethods);
        Result resultInvalidExpected = await paymentRep.RemoveAsync(user.Id, newPaymentM.Id);

        //Assert
        Assert.False(resultInvalidExpected.IsSuccess);
        Assert.Equal(PaymentMethodError.NotFound, resultInvalidExpected.Error);
    }


    [Fact]
    public async Task GetByIdTest()
    {
        using var context = ContextHelper.Create();
        PaymentMethodRepository paymentRep = PaymentMethodFactory.CreatePaymentMethodRepository(context);
        User user = ContextHelper.CreateUser(context);
        PaymentMethod paymentMethod1 = PaymentMethodFactory.CreateRandomPaymentMethod(context, user);
        PaymentMethod paymentMethod2 = PaymentMethodFactory.CreateRandomPaymentMethod(context, user);
        PaymentMethod paymentMethod3 = PaymentMethodFactory.CreateRandomPaymentMethod(context, user);

        //Act
        var result = await paymentRep.GetByIdAsync(user.Id, paymentMethod3.Id);

        //Assert
        Assert.True(result.IsSuccess);
        PaymentMethod? paymentMSearched = result.Value;
        Assert.NotNull(paymentMSearched);
        Assert.Equal(paymentMethod3.Token, paymentMSearched.Token);
    }


    [Fact]
    public async Task GetById_NotFound()
    {
        using var context = ContextHelper.Create();
        PaymentMethodRepository paymentRep = PaymentMethodFactory.CreatePaymentMethodRepository(context);
        User user = ContextHelper.CreateUser(context);
        User anotherUser = new("Carlita", "carlita@gmail.com");
        PaymentMethod anotherPaymentMethodUser = PaymentMethodFactory.CreateRandomPaymentMethod(context, anotherUser);

        //Act
        var result = await paymentRep.GetByIdAsync(user.Id, anotherPaymentMethodUser.Id);
        //Assert
        Assert.False(result.IsSuccess);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public async Task SetDefaultMultipleTimes()
    {
        using var context = ContextHelper.Create();
        PaymentMethodRepository paymentRep = PaymentMethodFactory.CreatePaymentMethodRepository(context);
        User user = ContextHelper.CreateUser(context);
        PaymentMethod paymentMethod1 = PaymentMethodFactory.CreateRandomPaymentMethod(context, user, IsDefault: true);
        PaymentMethod paymentMethod2 = PaymentMethodFactory.CreateRandomPaymentMethod(context, user, IsDefault: false);
        PaymentMethod paymentMethod3 = PaymentMethodFactory.CreateRandomPaymentMethod(context, user, IsDefault: true);


        //Act
        var result = await paymentRep.GetByIdAsync(user.Id, paymentMethod3.Id);
        //Assert
        Assert.True(result.IsSuccess);
        PaymentMethod currentDefaultPaymentM = result.Value;
        Assert.NotNull(currentDefaultPaymentM);
        Assert.Equal(paymentMethod3.Id, currentDefaultPaymentM.Id);
    }


    [Fact]
    public async Task SetAsDefault()
    {
        using var context = ContextHelper.Create();
        PaymentMethodRepository paymentRep = PaymentMethodFactory.CreatePaymentMethodRepository(context);
        User user = ContextHelper.CreateUser(context);
        PaymentMethod paymentMethod1 = PaymentMethodFactory.CreateRandomPaymentMethod(context, user, IsDefault: true);
        PaymentMethod paymentMethod2 = PaymentMethodFactory.CreateRandomPaymentMethod(context, user, IsDefault: true);
        PaymentMethod paymentMethod3 = PaymentMethodFactory.CreateRandomPaymentMethod(context, user, IsDefault: false);

        //Act
        var resultPmSearch = await paymentRep.GetByIdAsync(user.Id, paymentMethod3.Id);

        //Assert
        Assert.True(resultPmSearch.IsSuccess);
        PaymentMethod paymentMethodNotDefault = resultPmSearch.Value;
        Assert.NotNull(paymentMethodNotDefault);
        Assert.False(paymentMethodNotDefault.IsDefault);

        Result result = await paymentRep.SetAsDefaultAsync(user.Id, paymentMethod3.Id);
        Assert.True(result.IsSuccess);
        Assert.Single(user.PaymentMethods, p => p.IsDefault);
    }


    [Fact]
    public async Task SetAsDefault_NotFound()
    {
        using var context = ContextHelper.Create();
        PaymentMethodRepository paymentRep = PaymentMethodFactory.CreatePaymentMethodRepository(context);
        User userWithoutPM = ContextHelper.CreateUser(context);
        User userWithPM = new("Carlita", "carlita@gmail.com");
        PaymentMethod paymentMethod1 = PaymentMethodFactory.CreateRandomPaymentMethod(context, userWithPM, IsDefault: true);

        //Act
        Result result = await paymentRep.SetAsDefaultAsync(userWithoutPM.Id, paymentMethod1.Id);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PaymentMethodError.NotFound, result.Error);
    }

}
