using System.Net;
using System.Text;
using System.Text.Json;
using VerificahubNet;
using VerificahubNet.Models;
using VerificahubNet.Requests;

namespace VerificahubNet.Tests;

public sealed class VerificahubClientTests
{
    private const string ApiKey = "vh_live_key";
    private const string ApiSecret = "vh_sec_secret";

    private static VerificahubClient ClientWith(RecordingHandler handler)
        => new VerificahubClient(ApiKey, ApiSecret, new HttpClient(handler));

    private static string ExpectedBasicHeader()
        => "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(ApiKey + ":" + ApiSecret));

    [Fact]
    public async Task VerifyAsync_PostsBasicAuthAndSnakeCaseBody_ToVerifyEndpoint()
    {
        var handler = new RecordingHandler(ReverseFlashCallJson, HttpStatusCode.Created);
        using var client = ClientWith(handler);

        await client.VerifyAsync(new InitiateVerificationRequest("+79991234567"));

        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("https://api.verificahub.ru/v1/verify", handler.RequestUri!.ToString());
        Assert.Equal(ExpectedBasicHeader(), handler.AuthorizationHeader);

        using var doc = JsonDocument.Parse(handler.Body!);
        Assert.Equal("+79991234567", doc.RootElement.GetProperty("phone_number").GetString());
        Assert.Equal("reverse_flash_call", doc.RootElement.GetProperty("method").GetString());
    }

    [Fact]
    public async Task VerifyAsync_DeserializesPolymorphicReverseFlashCallResponse()
    {
        var handler = new RecordingHandler(ReverseFlashCallJson, HttpStatusCode.Created);
        using var client = ClientWith(handler);

        var response = await client.VerifyAsync(new InitiateVerificationRequest("+79991234567"));

        var flash = Assert.IsType<ReverseFlashCallInitiateResponse>(response);
        Assert.Equal(VerificationMethod.ReverseFlashCall, flash.Method);
        Assert.Equal("+74990000000", flash.NumberToCall);
        Assert.Equal(VerificationStatus.Sent, flash.Status);
        Assert.Equal(1.50m, flash.Cost.Amount);
        Assert.Equal("RUB", flash.Cost.Currency);
        Assert.Equal(4, flash.CodeLength);
    }

    [Fact]
    public async Task VerifyAsync_DeserializesPolymorphicResponse_WhenDiscriminatorIsNotFirst()
    {
        const string json = """
        {
          "request_id": "8f2c0000-0000-0000-0000-000000000001",
          "phone_number": "+79991234567",
          "status": "sent",
          "cost": { "amount": 2.00, "currency": "RUB" },
          "expires_at": "2026-06-21T10:00:00Z",
          "code_length": 6,
          "method": "telegram_otp"
        }
        """;
        var handler = new RecordingHandler(json, HttpStatusCode.Created);
        using var client = ClientWith(handler);

        var response = await client.VerifyAsync(new InitiateVerificationRequest("+79991234567") { Method = VerificationMethod.TelegramOtp });

        Assert.IsType<TelegramInitiateResponse>(response);
        Assert.Equal(VerificationMethod.TelegramOtp, response.Method);
    }

    [Fact]
    public async Task GetStatusAsync_GetsRequestById_AndParsesMaskedStatus()
    {
        const string json = """
        {
          "request_id": "8f2c0000-0000-0000-0000-000000000001",
          "phone_number": "+7999*****67",
          "method": "reverse_flash_call",
          "status": "verified",
          "cost": { "amount": 1.50, "currency": "RUB" },
          "created_at": "2026-06-21T09:30:00Z",
          "verified_at": "2026-06-21T09:30:42Z",
          "expires_at": "2026-06-21T09:35:00Z"
        }
        """;
        var handler = new RecordingHandler(json);
        using var client = ClientWith(handler);
        var id = Guid.Parse("8f2c0000-0000-0000-0000-000000000001");

        var status = await client.GetStatusAsync(id);

        Assert.Equal(HttpMethod.Get, handler.Method);
        Assert.Equal("https://api.verificahub.ru/v1/verify/8f2c0000-0000-0000-0000-000000000001", handler.RequestUri!.ToString());
        Assert.Equal(VerificationStatus.Verified, status.Status);
        Assert.Equal("+7999*****67", status.PhoneNumber);
        Assert.NotNull(status.VerifiedAt);
    }

    [Fact]
    public async Task CheckCodeAsync_InvalidCode_ThrowsApiExceptionWithAttemptsRemaining()
    {
        const string problem = """
        {
          "type": "https://httpstatuses.io/400",
          "title": "Bad Request",
          "status": 400,
          "detail": "The code is incorrect.",
          "error_code": "invalid_code",
          "trace_id": "trace-123",
          "attempts_remaining": 2
        }
        """;
        var handler = new RecordingHandler(problem, HttpStatusCode.BadRequest);
        using var client = ClientWith(handler);

        var ex = await Assert.ThrowsAsync<VerificahubApiException>(
            () => client.CheckCodeAsync(new CheckCodeRequest(Guid.NewGuid(), "0000")));

        Assert.Equal(400, ex.StatusCode);
        Assert.Equal(VerificahubErrorCodes.InvalidCode, ex.ErrorCode);
        Assert.Equal(2, ex.AttemptsRemaining);
        Assert.Equal("trace-123", ex.TraceId);
    }

    [Fact]
    public async Task GetBalanceAsync_ParsesMoney()
    {
        var handler = new RecordingHandler("""{ "balance": { "amount": 42.50, "currency": "RUB" } }""");
        using var client = ClientWith(handler);

        var balance = await client.GetBalanceAsync();

        Assert.Equal("https://api.verificahub.ru/v1/balance", handler.RequestUri!.ToString());
        Assert.Equal(42.50m, balance.Balance.Amount);
        Assert.Equal("RUB", balance.Balance.Currency);
    }

    [Fact]
    public async Task GetUsageAsync_BuildsDateQuery_AndParsesEnumKeyedDictionaries()
    {
        const string json = """
        {
          "from": "2026-06-14",
          "to": "2026-06-21",
          "total_verifications": 10,
          "successful": 8,
          "success_rate": 0.8,
          "total_cost": { "amount": 15.00, "currency": "RUB" },
          "by_method": { "reverse_flash_call": 9, "telegram_otp": 1 },
          "by_status": { "verified": 8, "expired": 2 },
          "daily": [ { "date": "2026-06-20", "total": 5, "verified": 4 } ]
        }
        """;
        var handler = new RecordingHandler(json);
        using var client = ClientWith(handler);

        var usage = await client.GetUsageAsync(new DateTime(2026, 6, 14), new DateTime(2026, 6, 21));

        Assert.Contains("from=2026-06-14", handler.RequestUri!.Query);
        Assert.Contains("to=2026-06-21", handler.RequestUri!.Query);
        Assert.Equal(9, usage.ByMethod[VerificationMethod.ReverseFlashCall]);
        Assert.Equal(1, usage.ByMethod[VerificationMethod.TelegramOtp]);
        Assert.Equal(8, usage.ByStatus[VerificationStatus.Verified]);
        Assert.Equal(new DateTime(2026, 6, 14), usage.From);
        var day = Assert.Single(usage.Daily);
        Assert.Equal(new DateTime(2026, 6, 20), day.Date);
        Assert.Equal(4, day.Verified);
    }

    [Fact]
    public async Task NonSuccess_WithoutProblemBody_ThrowsProtocolException()
    {
        var handler = new RecordingHandler("<html>502 Bad Gateway</html>", HttpStatusCode.BadGateway);
        using var client = ClientWith(handler);

        var ex = await Assert.ThrowsAsync<VerificahubProtocolException>(() => client.GetBalanceAsync());
        Assert.Equal(502, ex.StatusCode);
    }

    [Fact]
    public async Task TransportFailure_ThrowsTransportException()
    {
        using var client = new VerificahubClient(ApiKey, ApiSecret, new HttpClient(new ThrowingHandler(new HttpRequestException("boom"))));

        await Assert.ThrowsAsync<VerificahubTransportException>(() => client.GetBalanceAsync());
    }

    [Theory]
    [InlineData("", "secret")]
    [InlineData("key", "")]
    [InlineData(" ", "secret")]
    public void Constructor_RejectsMissingCredentials(string key, string secret)
    {
        Assert.Throws<ArgumentException>(() => new VerificahubClient(key, secret));
    }

    private const string ReverseFlashCallJson = """
    {
      "method": "reverse_flash_call",
      "request_id": "8f2c0000-0000-0000-0000-000000000001",
      "phone_number": "+79991234567",
      "status": "sent",
      "cost": { "amount": 1.50, "currency": "RUB" },
      "expires_at": "2026-06-21T10:00:00Z",
      "code_length": 4,
      "number_to_call": "+74990000000"
    }
    """;
}
