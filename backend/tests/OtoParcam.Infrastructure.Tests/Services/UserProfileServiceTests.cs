using OtoParcam.Application.Users;
using OtoParcam.Infrastructure.Services;
using static OtoParcam.Infrastructure.Tests.TestFixtures;

namespace OtoParcam.Infrastructure.Tests.Services;

public class UserProfileServiceTests
{
    [Fact]
    public async Task GetProfileAsync_UnknownUser_ReturnsNull()
    {
        await using var context = CreateContext();
        var service = new UserProfileService(context);

        var result = await service.GetProfileAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetProfileAsync_KnownUser_ReturnsMappedDto()
    {
        await using var context = CreateContext();
        var user = CreateUser("customer@test.com");
        user.PhoneNumber = "5551234567";
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new UserProfileService(context);
        var result = await service.GetProfileAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.FirstName, result!.FirstName);
        Assert.Equal(user.LastName, result.LastName);
        Assert.Equal("customer@test.com", result.Email);
        Assert.Equal("5551234567", result.PhoneNumber);
    }

    [Fact]
    public async Task GetProfileAsync_NullEmailAndPhone_MapToEmptyStrings()
    {
        await using var context = CreateContext();
        var user = CreateUser("customer@test.com");
        user.Email = null;
        user.PhoneNumber = null;
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new UserProfileService(context);
        var result = await service.GetProfileAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(string.Empty, result!.Email);
        Assert.Equal(string.Empty, result.PhoneNumber);
    }

    [Fact]
    public async Task UpdateProfileAsync_UnknownUser_ReturnsNull()
    {
        await using var context = CreateContext();
        var service = new UserProfileService(context);

        var result = await service.UpdateProfileAsync(Guid.NewGuid(), new UpdateUserProfileRequest { FirstName = "A", LastName = "B" });

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateProfileAsync_UpdatesNameOnly_LeavesEmailAndPhoneUnchanged()
    {
        await using var context = CreateContext();
        var user = CreateUser("customer@test.com");
        user.PhoneNumber = "5551234567";
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new UserProfileService(context);
        var result = await service.UpdateProfileAsync(user.Id, new UpdateUserProfileRequest { FirstName = "Ahmet", LastName = "Yilmaz" });

        Assert.NotNull(result);
        Assert.Equal("Ahmet", result!.FirstName);
        Assert.Equal("Yilmaz", result.LastName);
        Assert.Equal("customer@test.com", result.Email);
        Assert.Equal("5551234567", result.PhoneNumber);

        var stored = await context.Users.FindAsync(user.Id);
        Assert.Equal("Ahmet", stored!.FirstName);
        Assert.Equal("customer@test.com", stored.Email);
    }
}
