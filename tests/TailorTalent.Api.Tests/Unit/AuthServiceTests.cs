using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using TailorTalent.Api.Services;
using Xunit;

namespace TailorTalent.Api.Tests.Unit;

public class AuthServiceTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly AuthService _auth;

    public AuthServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "unit-test-signing-key-needs-32-bytes-min!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            })
            .Build();

        var subs = new SubscriptionService(_db.Context, NullLogger<SubscriptionService>.Instance);
        _auth = new AuthService(_db.Context, config, subs);
    }

    [Fact]
    public async Task Register_NewUser_ReturnsTokenAndInitializesCredits()
    {
        var result = await _auth.RegisterAsync("a@b.com", "Password1!", "Alice");

        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.Token));
        Assert.Equal("a@b.com", result.Email);

        // Registration must seed the free-tier credits
        var credits = _db.Context.UserCredits.Single(c => c.UserId == result.UserId);
        Assert.Equal(3, credits.CreditsRemaining);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Fails()
    {
        await _auth.RegisterAsync("dup@b.com", "Password1!", "Alice");
        var second = await _auth.RegisterAsync("dup@b.com", "Password2!", "Bob");

        Assert.False(second.Success);
        Assert.Contains("already registered", second.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_CorrectCredentials_Succeeds()
    {
        await _auth.RegisterAsync("login@b.com", "Password1!", "Alice");
        var result = await _auth.LoginAsync("login@b.com", "Password1!");

        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.Token));
    }

    [Theory]
    [InlineData("login2@b.com", "WrongPassword")]
    [InlineData("nosuchuser@b.com", "Password1!")]
    public async Task Login_BadCredentials_Fails(string email, string password)
    {
        await _auth.RegisterAsync("login2@b.com", "Password1!", "Alice");
        var result = await _auth.LoginAsync(email, password);

        Assert.False(result.Success);
        Assert.Equal("Invalid email or password.", result.Error);
    }

    [Fact]
    public async Task Token_ContainsExpectedClaims()
    {
        var result = await _auth.RegisterAsync("claims@b.com", "Password1!", "Alice");
        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);

        Assert.Equal("TestIssuer", token.Issuer);
        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Email && c.Value == "claims@b.com");
        Assert.Contains(token.Claims, c => c.Value == result.UserId);
        Assert.True(token.ValidTo > DateTime.UtcNow);
    }

    [Fact]
    public async Task GetProfile_InvalidGuid_ReturnsNull()
    {
        var profile = await _auth.GetProfileAsync("not-a-guid");
        Assert.Null(profile);
    }

    [Fact]
    public async Task GetProfile_ExistingUser_ReturnsProfile()
    {
        var reg = await _auth.RegisterAsync("prof@b.com", "Password1!", "Alice");
        var profile = await _auth.GetProfileAsync(reg.UserId);

        Assert.NotNull(profile);
        Assert.Equal("prof@b.com", profile!.Email);
        Assert.Equal("Alice", profile.FullName);
    }

    public void Dispose() => _db.Dispose();
}
