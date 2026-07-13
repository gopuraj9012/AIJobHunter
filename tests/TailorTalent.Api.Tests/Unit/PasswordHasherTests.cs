using TailorTalent.Api.Services;
using Xunit;

namespace TailorTalent.Api.Tests.Unit;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_ThenVerify_Succeeds()
    {
        var hash = BCryptPasswordHasher.Hash("S3cret!pass");
        Assert.True(BCryptPasswordHasher.Verify("S3cret!pass", hash));
    }

    [Fact]
    public void Verify_WrongPassword_Fails()
    {
        var hash = BCryptPasswordHasher.Hash("S3cret!pass");
        Assert.False(BCryptPasswordHasher.Verify("wrong-password", hash));
    }

    [Fact]
    public void Hash_IsNotPlaintext()
    {
        var hash = BCryptPasswordHasher.Hash("S3cret!pass");
        Assert.NotEqual("S3cret!pass", hash);
        Assert.DoesNotContain("S3cret", hash);
    }
}
