using System.Net;
using System.Text.Json;
using VerificahubNet;
using VerificahubNet.Models;
using VerificahubNet.Requests;

namespace VerificahubNet.Tests;

/// <summary>
/// Pins the SDK to the shapes the API currently sends, so a method or field added server-side is
/// caught here rather than by an integrator.
/// </summary>
public sealed class ContractTests
{
    private static VerificahubClient ClientWith(RecordingHandler handler)
        => new VerificahubClient("vh_live_key", "vh_sec_secret", new HttpClient(handler));

    private static string InitiateJson(string method, string extra = "") => $$"""
        {
          "method": "{{method}}",
          "request_id": "8f2c0000-0000-0000-0000-000000000001",
          "phone_number": "+79991234567",
          "status": "sent",
          "cost": { "amount": 3.50, "currency": "RUB" },
          "expires_at": "2026-06-21T10:00:00Z",
          "code_length": 4{{extra}}
        }
        """;

    [Fact]
    public async Task EveryKnownMethod_DeserializesToItsOwnTypedResponse()
    {
        // A method in the enum without a converter entry would surface as UnknownInitiateResponse —
        // exactly the drift that left whatsapp_otp and pass_call unusable in 1.x.
        foreach (VerificationMethod method in Enum.GetValues<VerificationMethod>())
        {
            if (method == VerificationMethod.Unknown)
            {
                continue;
            }

            // The enum carries its own converter, so default options yield the wire name.
            string wire = JsonSerializer.Serialize(method).Trim('"');
            using var client = ClientWith(new RecordingHandler(InitiateJson(wire), HttpStatusCode.Created));

            var response = await client.VerifyAsync(new InitiateVerificationRequest("+79991234567") { Method = method });

            Assert.IsNotType<UnknownInitiateResponse>(response);
            Assert.Equal(method, response.Method);
        }
    }

    [Theory]
    [InlineData(VerificationMethod.WhatsappOtp, "whatsapp_otp", typeof(WhatsAppInitiateResponse))]
    [InlineData(VerificationMethod.PassCall, "pass_call", typeof(PassCallInitiateResponse))]
    public async Task VerifyAsync_SendsAndParsesNewMethods(VerificationMethod method, string wire, Type expected)
    {
        var handler = new RecordingHandler(InitiateJson(wire), HttpStatusCode.Created);
        using var client = ClientWith(handler);

        var response = await client.VerifyAsync(new InitiateVerificationRequest("+79991234567") { Method = method });

        using var doc = JsonDocument.Parse(handler.Body!);
        Assert.Equal(wire, doc.RootElement.GetProperty("method").GetString());
        Assert.IsType(expected, response);
        Assert.Equal(method, response.Method);
        Assert.Equal(4, response.CodeLength);
    }

    [Fact]
    public async Task VerifyAsync_UnrecognisedMethod_ReturnsUnknownResponse_InsteadOfThrowing()
    {
        // The verification already exists server-side; losing its request_id to an exception would
        // leave the caller unable to track or check it.
        var handler = new RecordingHandler(InitiateJson("carrier_pigeon", ""","wings": 2"""), HttpStatusCode.Created);
        using var client = ClientWith(handler);

        var response = await client.VerifyAsync(new InitiateVerificationRequest("+79991234567"));

        var unknown = Assert.IsType<UnknownInitiateResponse>(response);
        Assert.Equal(VerificationMethod.Unknown, unknown.Method);
        Assert.Equal("carrier_pigeon", unknown.RawMethod);
        Assert.Equal(Guid.Parse("8f2c0000-0000-0000-0000-000000000001"), unknown.RequestId);
        Assert.Equal(3.50m, unknown.Cost.Amount);
    }

    [Fact]
    public async Task VerifyAsync_MissingMethod_ReturnsUnknownResponse_WithNullRawMethod()
    {
        const string json = """
        { "request_id": "8f2c0000-0000-0000-0000-000000000001", "phone_number": "+79991234567", "status": "sent",
          "cost": { "amount": 1.00, "currency": "RUB" }, "expires_at": "2026-06-21T10:00:00Z", "code_length": 4 }
        """;
        using var client = ClientWith(new RecordingHandler(json, HttpStatusCode.Created));

        var unknown = Assert.IsType<UnknownInitiateResponse>(await client.VerifyAsync(new InitiateVerificationRequest("+79991234567")));

        Assert.Null(unknown.RawMethod);
    }

    [Theory]
    [InlineData("\"no_response\"", VerificationFailureReason.NoResponse)]
    [InlineData("\"declined\"", VerificationFailureReason.Declined)]
    [InlineData("\"wrong_code\"", VerificationFailureReason.WrongCode)]
    [InlineData("\"not_delivered\"", VerificationFailureReason.NotDelivered)]
    [InlineData("\"unreachable\"", VerificationFailureReason.Unreachable)]
    [InlineData("\"unknown\"", VerificationFailureReason.Unknown)]
    [InlineData("\"something_new\"", VerificationFailureReason.Unknown)]
    [InlineData("null", null)]
    public async Task GetStatusAsync_ParsesFailureReason(string wire, VerificationFailureReason? expected)
    {
        string json = $$"""
        {
          "request_id": "8f2c0000-0000-0000-0000-000000000001",
          "phone_number": "+7999*****67",
          "method": "sms",
          "status": "expired",
          "awaiting_code": false,
          "cost": { "amount": 0, "currency": "RUB" },
          "created_at": "2026-06-21T09:55:00Z",
          "verified_at": null,
          "expires_at": "2026-06-21T10:00:00Z",
          "failure_reason": {{wire}}
        }
        """;
        using var client = ClientWith(new RecordingHandler(json));

        var status = await client.GetStatusAsync(Guid.Parse("8f2c0000-0000-0000-0000-000000000001"));

        Assert.Equal(expected, status.FailureReason);
    }

    [Fact]
    public async Task GetStatusAsync_FailureReasonAbsent_IsNull()
    {
        const string json = """
        { "request_id": "8f2c0000-0000-0000-0000-000000000001", "phone_number": "+7999*****67", "method": "sms",
          "status": "sent", "awaiting_code": true, "cost": { "amount": 3.5, "currency": "RUB" },
          "created_at": "2026-06-21T09:55:00Z", "expires_at": "2026-06-21T10:00:00Z" }
        """;
        using var client = ClientWith(new RecordingHandler(json));

        var status = await client.GetStatusAsync(Guid.NewGuid());

        Assert.Null(status.FailureReason);
    }

    [Fact]
    public async Task GetPricesAsync_ParsesAvailability_DefaultingToTrue()
    {
        const string json = """
        {
          "prices": [
            { "method": "sms",       "price": { "amount": 3.50, "currency": "RUB" }, "available": true },
            { "method": "pass_call", "price": { "amount": 4.00, "currency": "RUB" }, "available": false },
            { "method": "max_bot",   "price": { "amount": 0.50, "currency": "RUB" } }
          ]
        }
        """;
        using var client = ClientWith(new RecordingHandler(json));

        var prices = await client.GetPricesAsync();

        Assert.Collection(prices.Prices,
            p => { Assert.Equal(VerificationMethod.Sms, p.Method); Assert.True(p.Available); },
            p => { Assert.Equal(VerificationMethod.PassCall, p.Method); Assert.False(p.Available); },
            p => { Assert.Equal(VerificationMethod.MaxBot, p.Method); Assert.True(p.Available); });
    }

    [Fact]
    public async Task ComplianceRequired_ExposesComplianceStatusAndDashboardUrl()
    {
        const string json = """
        { "status": 403, "title": "Forbidden", "detail": "Project verification is required.",
          "error_code": "compliance_required", "trace_id": "00-abc-def-00",
          "compliance_status": "submitted", "dashboard_url": "https://app.verificahub.ru/compliance" }
        """;
        using var client = ClientWith(new RecordingHandler(json, HttpStatusCode.Forbidden));

        var ex = await Assert.ThrowsAsync<VerificahubApiException>(() => client.GetBalanceAsync());

        Assert.Equal(403, ex.StatusCode);
        Assert.Equal(VerificahubErrorCodes.ComplianceRequired, ex.ErrorCode);
        Assert.Equal("submitted", ex.Problem!.ComplianceStatus);
        Assert.Equal("https://app.verificahub.ru/compliance", ex.Problem.DashboardUrl);
    }

    [Fact]
    public async Task IpNotAllowed_ExposesSourceIp()
    {
        const string json = """
        { "status": 403, "error_code": "ip_not_allowed", "detail": "Address not approved.", "source_ip": "203.0.113.7" }
        """;
        using var client = ClientWith(new RecordingHandler(json, HttpStatusCode.Forbidden));

        var ex = await Assert.ThrowsAsync<VerificahubApiException>(() => client.GetBalanceAsync());

        Assert.Equal(VerificahubErrorCodes.IpNotAllowed, ex.ErrorCode);
        Assert.Equal("203.0.113.7", ex.Problem!.SourceIp);
    }

    [Fact]
    public void RetainedMethods_KeepTheirNumericValues()
    {
        // Integrators may have persisted these as integers; 2.0 removed members without renumbering.
        Assert.Equal(1, (int)VerificationMethod.ReverseFlashCall);
        Assert.Equal(2, (int)VerificationMethod.FlashCall);
        Assert.Equal(4, (int)VerificationMethod.Sms);
        Assert.Equal(5, (int)VerificationMethod.TelegramOtp);
        Assert.Equal(6, (int)VerificationMethod.MtsId);
        Assert.Equal(8, (int)VerificationMethod.MaxBot);
    }
}
