using TourPlannerAPI.Utilities;

namespace TourPlannerAPI.Tests;

[TestFixture]
public class PasswordHelperTests
{
    [Test]
    public void HashPassword_ProducesHashDifferentFromInput()
    {
        var hash = PasswordHelper.HashPassword("Secret123");
        Assert.That(hash, Is.Not.EqualTo("Secret123"));
        Assert.That(hash, Is.Not.Empty);
    }

    [Test]
    public void HashPassword_IsSaltedSoTwoHashesDiffer()
    {
        var a = PasswordHelper.HashPassword("Secret123");
        var b = PasswordHelper.HashPassword("Secret123");
        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void VerifyPassword_ReturnsTrueForCorrectPassword()
    {
        var hash = PasswordHelper.HashPassword("Secret123");
        Assert.That(PasswordHelper.VerifyPassword("Secret123", hash), Is.True);
    }

    [Test]
    public void VerifyPassword_ReturnsFalseForWrongPassword()
    {
        var hash = PasswordHelper.HashPassword("Secret123");
        Assert.That(PasswordHelper.VerifyPassword("WrongPassword", hash), Is.False);
    }

    [Test]
    public void VerifyPassword_ReturnsFalseForInvalidStoredHash()
    {
        Assert.That(PasswordHelper.VerifyPassword("Secret123", "not-a-bcrypt-hash"), Is.False);
    }

    [Test]
    public void HashPassword_ThrowsForEmptyPassword()
    {
        Assert.That(() => PasswordHelper.HashPassword(""), Throws.ArgumentException);
    }
}
