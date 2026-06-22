using System.Security.Cryptography;
using System.Text;
using VerificahubNet.Models;
using VerificahubNet.Webhooks;

namespace VerificahubNet.Tests;

public sealed class WebhookSignatureValidatorTests
{
    private const string Secret = "vh_sec_secret";

    private const string Body = """
    {"event":"verification.verified","request_id":"8f2c0000-0000-0000-0000-000000000001","phone_number":"+7999*****67","method":"reverse_flash_call","status":"verified","timestamp":"2026-06-21T09:30:42Z"}
    """;

    // Independently reproduces the server's algorithm: sha256= + lowercase hex HMAC-SHA256(api_secret, body).
    private static string ServerSignature(string secret, string body)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
        return "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();
    }

    [Fact]
    public void ComputeSignature_MatchesServerAlgorithm()
    {
        var validator = new WebhookSignatureValidator(Secret);
        Assert.Equal(ServerSignature(Secret, Body), validator.ComputeSignature(Body));
    }

    [Fact]
    public void IsValid_CorrectSignature_ReturnsTrue()
    {
        var validator = new WebhookSignatureValidator(Secret);
        Assert.True(validator.IsValid(Body, ServerSignature(Secret, Body)));
    }

    [Fact]
    public void IsValid_IsCaseInsensitiveOnHex()
    {
        var validator = new WebhookSignatureValidator(Secret);
        string upper = ServerSignature(Secret, Body).ToUpperInvariant();
        Assert.True(validator.IsValid(Body, upper));
    }

    [Fact]
    public void IsValid_TamperedBody_ReturnsFalse()
    {
        var validator = new WebhookSignatureValidator(Secret);
        string signature = ServerSignature(Secret, Body);
        Assert.False(validator.IsValid(Body + " ", signature));
    }

    [Fact]
    public void IsValid_WrongSecret_ReturnsFalse()
    {
        var validator = new WebhookSignatureValidator(Secret);
        Assert.False(validator.IsValid(Body, ServerSignature("another_secret", Body)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsValid_MissingSignature_ReturnsFalse(string? signature)
    {
        var validator = new WebhookSignatureValidator(Secret);
        Assert.False(validator.IsValid(Body, signature));
    }

    [Fact]
    public void TryParse_ValidSignature_ParsesTypedEvent()
    {
        var validator = new WebhookSignatureValidator(Secret);

        bool ok = validator.TryParse(Body, ServerSignature(Secret, Body), out WebhookEvent? evt);

        Assert.True(ok);
        Assert.NotNull(evt);
        Assert.Equal(WebhookEventTypes.VerificationVerified, evt!.Event);
        Assert.Equal(Guid.Parse("8f2c0000-0000-0000-0000-000000000001"), evt.RequestId);
        Assert.Equal(VerificationMethod.ReverseFlashCall, evt.Method);
        Assert.Equal(VerificationStatus.Verified, evt.Status);
    }

    [Fact]
    public void TryParse_InvalidSignature_ReturnsFalseAndNull()
    {
        var validator = new WebhookSignatureValidator(Secret);

        bool ok = validator.TryParse(Body, "sha256=deadbeef", out WebhookEvent? evt);

        Assert.False(ok);
        Assert.Null(evt);
    }

    [Fact]
    public void Constructor_RejectsEmptySecret()
    {
        Assert.Throws<ArgumentException>(() => new WebhookSignatureValidator(""));
    }
}
