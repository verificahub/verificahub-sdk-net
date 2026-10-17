using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VerificahubNet;
using VerificahubNet.Models;
using VerificahubNet.Requests;
using Xunit;

namespace VerificahubNet.Tests;

/// <summary>
/// The number-lookup calls, and the fields the account endpoints grew alongside them.
/// </summary>
/// <remarks>
/// Response bodies here are copied from live production responses rather than written from the
/// documentation, which is the difference that has caught every wire-format surprise in this
/// codebase so far.
/// </remarks>
public sealed class LookupTests
{
    private const string ApiKey = "vh_live_key";
    private const string ApiSecret = "vh_sec_secret";

    private static VerificahubClient ClientWith(RecordingHandler handler)
        => new VerificahubClient(ApiKey, ApiSecret, new HttpClient(handler));

    /// <summary>A live <c>number-info</c> answer for an MTS number.</summary>
    private const string NumberInfoJson = """
        {"request_id":"d24e426e-14e5-4353-a781-bea74ea69cec","phone_number":"+7913*****01",
         "method":"number_info","status":"ok","operator":"mts","region_code":4,"score":null,
         "ported":false,"cost":{"amount":0.80,"currency":"RUB"},
         "checked_at":"2026-10-07T17:18:37.1701785+00:00"}
        """;

    /// <summary>A live <c>activity</c> answer, and the same number with nothing on record.</summary>
    private const string ActivityJson = """
        {"request_id":"f30b47e5-dfff-42e0-83e3-cbd53cc70f54","phone_number":"+7927*****62",
         "method":"activity_score","status":"ok","operator":"megafon","region_code":2,"score":0.96,
         "ported":null,"cost":{"amount":1.50,"currency":"RUB"},
         "checked_at":"2026-10-07T14:39:35.8374412+00:00"}
        """;

    private const string NoDataJson = """
        {"request_id":"d31cf90d-363d-4b64-8e2e-5995cb28ca32","phone_number":"+7913*****01",
         "method":"activity_score","status":"no_data","operator":null,"region_code":null,
         "score":null,"ported":null,"cost":{"amount":0,"currency":"RUB"},
         "checked_at":"2026-10-07T17:18:36.421737+00:00"}
        """;

    [Fact]
    public async Task LookupNumberInfoAsync_PostsToTheNumberInfoEndpoint()
    {
        var handler = new RecordingHandler(NumberInfoJson, HttpStatusCode.OK);
        using var client = ClientWith(handler);

        await client.LookupNumberInfoAsync(new LookupRequest { PhoneNumber = "+79139999901" });

        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("https://api.verificahub.ru/v1/lookup/number-info", handler.RequestUri!.ToString());
        Assert.Equal(
            "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(ApiKey + ":" + ApiSecret)),
            handler.AuthorizationHeader);

        using var doc = JsonDocument.Parse(handler.Body!);
        Assert.Equal("+79139999901", doc.RootElement.GetProperty("phone_number").GetString());
    }

    [Fact]
    public async Task LookupNumberInfoAsync_ReadsOperatorRegionAndPorting()
    {
        using var client = ClientWith(new RecordingHandler(NumberInfoJson, HttpStatusCode.OK));

        var result = await client.LookupNumberInfoAsync(new LookupRequest { PhoneNumber = "+79139999901" });

        Assert.Equal(LookupMethod.NumberInfo, result.Method);
        Assert.Equal(LookupStatus.Ok, result.Status);
        Assert.Equal(MobileOperator.Mts, result.Operator);
        Assert.Equal(4, result.RegionCode);
        Assert.False(result.Ported);
        Assert.Equal(0.80m, result.Cost.Amount);
        Assert.Equal("+7913*****01", result.PhoneNumber);
    }

    [Fact]
    public async Task LookupActivityAsync_PostsToTheActivityEndpointAndReadsTheScore()
    {
        var handler = new RecordingHandler(ActivityJson, HttpStatusCode.OK);
        using var client = ClientWith(handler);

        var result = await client.LookupActivityAsync(new LookupRequest { PhoneNumber = "+79279383562" });

        Assert.Equal("https://api.verificahub.ru/v1/lookup/activity", handler.RequestUri!.ToString());
        Assert.Equal(LookupMethod.ActivityScore, result.Method);
        Assert.Equal(MobileOperator.Megafon, result.Operator);
        Assert.Equal(0.96m, result.Score);
        Assert.Equal(1.50m, result.Cost.Amount);
    }

    /// <summary>
    /// "No data" is a <c>200</c>, not an error, and it is free. A client that threw here — or that
    /// billed for it — would misrepresent the service.
    /// </summary>
    [Fact]
    public async Task NoDataIsAnAnswerAndCostsNothing()
    {
        using var client = ClientWith(new RecordingHandler(NoDataJson, HttpStatusCode.OK));

        var result = await client.LookupActivityAsync(new LookupRequest { PhoneNumber = "+79139999901" });

        Assert.Equal(LookupStatus.NoData, result.Status);
        Assert.Null(result.Operator);
        Assert.Null(result.Score);
        Assert.Equal(0m, result.Cost.Amount);
    }

    [Fact]
    public async Task LookupRequiresARequest()
    {
        using var client = ClientWith(new RecordingHandler(NumberInfoJson, HttpStatusCode.OK));

        await Assert.ThrowsAsync<ArgumentNullException>(() => client.LookupNumberInfoAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => client.LookupActivityAsync(null!));
    }

    /// <summary>An unfamiliar service or outcome must not throw — new ones ship without an SDK release.</summary>
    [Fact]
    public async Task UnknownServiceOrStatusFallsBackInsteadOfThrowing()
    {
        const string json = """
            {"request_id":"00000000-0000-0000-0000-000000000001","phone_number":"+7913*****01",
             "method":"something_new","status":"partially_known",
             "cost":{"amount":0,"currency":"RUB"},"checked_at":"2026-10-07T17:18:36Z"}
            """;
        using var client = ClientWith(new RecordingHandler(json, HttpStatusCode.OK));

        var result = await client.LookupNumberInfoAsync(new LookupRequest { PhoneNumber = "+79139999901" });

        Assert.Equal(LookupMethod.Unknown, result.Method);
        Assert.Equal(LookupStatus.Unknown, result.Status);
    }

    // ── the fields the account endpoints grew ────────────────────────────────

    [Fact]
    public async Task GetPricesAsync_ReadsTheLookupTariffsAlongsideTheMethods()
    {
        const string json = """
            {"prices":[{"method":"max_otp","price":{"amount":2.00,"currency":"RUB"}}],
             "lookups":[{"method":"number_info","price":{"amount":0.80,"currency":"RUB"}},
                        {"method":"activity_score","price":{"amount":1.50,"currency":"RUB"}}]}
            """;
        using var client = ClientWith(new RecordingHandler(json, HttpStatusCode.OK));

        var prices = await client.GetPricesAsync();

        Assert.Equal(VerificationMethod.MaxOtp, Assert.Single(prices.Prices).Method);
        Assert.Equal(2, prices.Lookups.Count);
        Assert.Equal(LookupMethod.NumberInfo, prices.Lookups[0].Method);
        Assert.Equal(1.50m, prices.Lookups[1].Price.Amount);
    }

    /// <summary>
    /// Lookups are reported separately from verifications, so spend for a range is the two totals
    /// added together.
    /// </summary>
    [Fact]
    public async Task GetUsageAsync_ReadsTheSeparateLookupTotals()
    {
        const string json = """
            {"from":"2026-09-15","to":"2026-10-14","total_verifications":128,"successful":121,
             "success_rate":0.9453,"total_cost":{"amount":64.00,"currency":"RUB"},
             "by_method":{},"by_status":{},"daily":[],
             "lookups":{"total":40,"billable":37,"total_cost":{"amount":31.10,"currency":"RUB"},
                        "by_method":{"number_info":25},"by_status":{"ok":37,"no_data":3}}}
            """;
        using var client = ClientWith(new RecordingHandler(json, HttpStatusCode.OK));

        var usage = await client.GetUsageAsync();

        Assert.Equal(64.00m, usage.TotalCost.Amount);
        Assert.Equal(40, usage.Lookups.Total);
        Assert.Equal(37, usage.Lookups.Billable);       // three found nothing and were free
        Assert.Equal(31.10m, usage.Lookups.TotalCost.Amount);
        Assert.Equal(25, usage.Lookups.ByMethod[LookupMethod.NumberInfo]);
        Assert.Equal(3, usage.Lookups.ByStatus[LookupStatus.NoData]);
    }

    /// <summary>
    /// A credited account may spend below zero, so <c>available</c> is what a request is checked
    /// against — not <c>balance</c>.
    /// </summary>
    [Fact]
    public async Task GetBalanceAsync_ReadsTheCreditLimitAndWhatIsActuallySpendable()
    {
        const string json = """
            {"balance":{"amount":-250.00,"currency":"RUB"},
             "credit_limit":{"amount":1000.00,"currency":"RUB"},
             "available":{"amount":750.00,"currency":"RUB"}}
            """;
        using var client = ClientWith(new RecordingHandler(json, HttpStatusCode.OK));

        var balance = await client.GetBalanceAsync();

        Assert.Equal(-250.00m, balance.Balance.Amount);
        Assert.Equal(1000.00m, balance.CreditLimit!.Amount);
        Assert.Equal(750.00m, balance.Available.Amount);
    }

    /// <summary>An account without a credit limit reports none, and stops at zero.</summary>
    [Fact]
    public async Task GetBalanceAsync_LeavesTheCreditLimitNullWhenThereIsNone()
    {
        const string json = """
            {"balance":{"amount":94.00,"currency":"RUB"},"credit_limit":null,
             "available":{"amount":94.00,"currency":"RUB"}}
            """;
        using var client = ClientWith(new RecordingHandler(json, HttpStatusCode.OK));

        var balance = await client.GetBalanceAsync();

        Assert.Null(balance.CreditLimit);
        Assert.Equal(balance.Balance.Amount, balance.Available.Amount);
    }

    /// <summary>
    /// MAX and MAX Bot are different methods at different prices, and the names make them easy to
    /// confuse — a client that collapsed them would bill the wrong one.
    /// </summary>
    [Fact]
    public async Task BothMaxMethodsAreDistinct()
    {
        const string json = """
            {"prices":[{"method":"max_otp","price":{"amount":2.00,"currency":"RUB"}},
                       {"method":"max_bot","price":{"amount":0.30,"currency":"RUB"}}],
             "lookups":[]}
            """;
        using var client = ClientWith(new RecordingHandler(json, HttpStatusCode.OK));

        var prices = await client.GetPricesAsync();

        Assert.Equal(VerificationMethod.MaxOtp, prices.Prices[0].Method);
        Assert.Equal(2.00m, prices.Prices[0].Price.Amount);
        Assert.Equal(VerificationMethod.MaxBot, prices.Prices[1].Method);
        Assert.Equal(0.30m, prices.Prices[1].Price.Amount);
    }
}
