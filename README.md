<table>
  <tr>
    <td width="170" align="center" valign="middle">
      <img src="https://raw.githubusercontent.com/verificahub/verificahub-sdk-net/main/assets/icon.png" width="140" alt="VerificahubNet logo" />
    </td>
    <td valign="middle">
      <h1>VerificahubNet</h1>
      <p>.NET SDK for the <a href="https://verificahub.ru">Verificahub</a> phone-verification API — SMS, flash call, reverse flash call, Telegram, MTS ID, and MAX verification, with typed models and webhook signature validation.</p>
      <p>
        <a href="https://github.com/verificahub/verificahub-sdk-net/actions/workflows/ci.yml"><img src="https://github.com/verificahub/verificahub-sdk-net/actions/workflows/ci.yml/badge.svg?branch=main" alt="CI" /></a>
        <a href="https://github.com/verificahub/verificahub-sdk-net/actions/workflows/release.yml"><img src="https://github.com/verificahub/verificahub-sdk-net/actions/workflows/release.yml/badge.svg" alt="Release" /></a>
        <a href="https://github.com/verificahub/verificahub-sdk-net/blob/main/LICENSE"><img src="https://img.shields.io/github/license/verificahub/verificahub-sdk-net?style=flat-square" alt="License" /></a>
        <a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/targets-netstandard2.0%20%7C%20net8.0%20%7C%20net10.0-512BD4?logo=dotnet&amp;style=flat-square" alt="Targets" /></a>
      </p>
      <p>
        <a href="https://www.nuget.org/packages/VerificahubNet"><img src="https://img.shields.io/nuget/v/VerificahubNet?logo=nuget&amp;style=flat-square" alt="NuGet version" /></a>
        <a href="https://www.nuget.org/packages/VerificahubNet"><img src="https://img.shields.io/nuget/dt/VerificahubNet?style=flat-square" alt="NuGet downloads" /></a>
      </p>
    </td>
  </tr>
</table>

The SDK covers the external integrator API (`/v1/*`) — initiate verifications, check codes, poll
status, read balance and usage — plus webhook signature validation, and follows Semantic Versioning.

> **Methods:** SMS, flash call, reverse flash call, Telegram, MTS ID, and MAX. **Region:** Russia (`+7`).

## Installation

```bash
dotnet add package VerificahubNet
```

The library multi-targets `netstandard2.0`, `net8.0`, and `net10.0`, so it runs on .NET Framework
4.6.1+, .NET Core 2.0+, Mono, Unity, and modern .NET.

## Quick Start

Obtain an `api_key` and `api_secret` from your Verificahub dashboard, then:

```csharp
using VerificahubNet;
using VerificahubNet.Models;
using VerificahubNet.Requests;

using var client = new VerificahubClient("vh_live_...", "vh_sec_...");

var verification = await client.VerifyAsync(new InitiateVerificationRequest("+79991234567"));

if (verification is ReverseFlashCallInitiateResponse flash)
    Console.WriteLine($"Ask the user to dial {flash.NumberToCall} from the number being verified.");
```

Credentials are sent as an `Authorization: Basic base64(api_key:api_secret)` header on every request.

## Initiating and Completing a Verification

`reverse_flash_call` completes automatically — the user dials a number we hand out and we verify by
caller id, so there is no code to submit. Poll for completion:

```csharp
var status = await client.GetStatusAsync(verification.RequestId);

bool done = status.Status == VerificationStatus.Verified;
```

The code-based methods (`sms`, `flash_call`, `telegram_otp`) deliver a code the user enters; submit
it with `CheckCodeAsync`:

```csharp
var result = await client.CheckCodeAsync(new CheckCodeRequest(verification.RequestId, "123456"));
bool verified = result.Status == VerificationStatus.Verified;
```

### MTS ID

`mts_id` pushes a silent confirmation to the user's SIM — nothing is typed on that leg and the result
arrives over the notification webhook (`verification.verified` / `verification.failed`), so the
initiate response starts as `VerificationStatus.Sent`. If the push can't be delivered the flow falls
back to SMS-OTP: a code becomes expected, signalled by the `verification.code_required` webhook and by
`AwaitingCode` on the status response. Prompt for the code only then, and submit it with `CheckCodeAsync`:

```csharp
var status = await client.GetStatusAsync(verification.RequestId);
if (status.AwaitingCode) // MTS ID fell back to SMS-OTP — a code is now expected
    await client.CheckCodeAsync(new CheckCodeRequest(verification.RequestId, "123456"));
```

### MAX

There are two MAX methods and they are not interchangeable.

`max_otp` sends the code straight to the user's number as a MAX message — nothing to open, no bot and
no link. The user types it back through `CheckCodeAsync`, exactly as with SMS. Delivery is synchronous,
so a number that is not on MAX fails immediately and is not charged.

`max_bot` is cheaper but costs the user a step: it returns a MAX deep link
(`MaxBotInitiateResponse.DeepLink`) to show as a link or QR. They open the bot, share their number, and
the bot delivers the code. Poll `AwaitingCode` (or handle the `verification.delivered` webhook) to know
when it arrived, then submit it with `CheckCodeAsync`:

```csharp
if (verification is MaxBotInitiateResponse max)
    Console.WriteLine($"Open the MAX bot: {max.DeepLink}");
```

## Number lookups

A lookup is **not** a verification: it reads what the operator's registry says about a number and proves
nothing about who is holding the phone. No session, no code, no webhook — and you are charged per
answer.

```csharp
var info = await client.LookupNumberInfoAsync(new LookupRequest { PhoneNumber = "+79991234567" });
if (info.Status == LookupStatus.Ok)
    Console.WriteLine($"{info.Operator}, region {info.RegionCode}, ported: {info.Ported}");

var activity = await client.LookupActivityAsync(new LookupRequest { PhoneNumber = "+79991234567" });
Console.WriteLine($"score: {activity.Score}");   // 0 to 1
```

Worth knowing before you build on it:

- **"No data" is an answer, not an error.** It comes back as `LookupStatus.NoData` with `Cost.Amount`
  of zero. Retrying will not change it, and a number with no activity score may still answer
  `LookupNumberInfoAsync` — that describes the service for that number, not the number.
- **Reconcile against `Cost`, not against the number of requests you made.**
- **The operator is the one serving the number now**, which is not what its prefix implies: a ported
  number keeps the prefix of the operator that issued it. That is the reason to ask rather than infer.
- **Treat the activity score as a signal, not a verdict.** The same number can score differently on
  repeated lookups, so it is sound for cleaning a list and unsound as the sole basis for refusing a
  customer.

## Balance, prices and usage

```csharp
var balance = await client.GetBalanceAsync();
// Check Available, not Balance: an account with an agreed credit limit may spend below zero, and a
// request costing more than Available is refused with insufficient_balance.
Console.WriteLine($"{balance.Available.Amount} {balance.Available.Currency} left to spend");

var prices = await client.GetPricesAsync();
foreach (var p in prices.Prices)
    Console.WriteLine($"{p.Method}: {p.Price.Amount} {p.Price.Currency}");
foreach (var l in prices.Lookups)
    Console.WriteLine($"lookup {l.Method}: {l.Price.Amount} {l.Price.Currency}");

var usage = await client.GetUsageAsync(from: DateTime.UtcNow.AddDays(-7), to: DateTime.UtcNow);
Console.WriteLine($"{usage.Successful}/{usage.TotalVerifications} verified");
// Lookups are counted separately, so spend for the range is the two totals added together.
Console.WriteLine($"{usage.Lookups.Billable}/{usage.Lookups.Total} lookups charged");
```

## Receiving Webhooks

Configure a webhook URL in the dashboard. Verificahub POSTs a `verification.*` event to it on every
status change, signed with your `api_secret`. Verify the `X-Verificahub-Signature` header against the
**exact, unparsed** request body:

```csharp
using VerificahubNet.Webhooks;

var validator = new WebhookSignatureValidator("vh_sec_...");

// rawBody is the raw request body; signature is the X-Verificahub-Signature header value.
if (validator.TryParse(rawBody, signature, out var evt))
{
    // evt is authentic
    Console.WriteLine($"{evt!.Event} for {evt.RequestId}: {evt.Status}");
}
else
{
    // Reject — the signature did not match.
}
```

The signature is `sha256=` + the lowercase hex `HMAC-SHA256(api_secret, raw_body)`, compared in
constant time. Webhook delivery is **at-least-once** — dedupe on `(request_id, event)` and treat the
polled `GetStatusAsync` as the source of truth if you ever need to reconcile.

## Error Handling

The SDK throws a small, typed hierarchy:

- **`VerificahubApiException`** — the API returned a non-2xx response. Branch on `ErrorCode` (see
  `VerificahubErrorCodes`); `StatusCode`, `Detail`, `TraceId`, and (for `invalid_code`)
  `AttemptsRemaining` are available.
- **`VerificahubTransportException`** — a network/DNS/TLS/timeout failure; no response was received.
- **`VerificahubProtocolException`** — a response arrived but could not be interpreted.

All three derive from `VerificahubException`.

```csharp
try
{
    var result = await client.CheckCodeAsync(new CheckCodeRequest(requestId, code));
}
catch (VerificahubApiException ex) when (ex.ErrorCode == VerificahubErrorCodes.InvalidCode)
{
    Console.WriteLine($"Wrong code, {ex.AttemptsRemaining} attempts left");
}
catch (VerificahubApiException ex) when (ex.ErrorCode == VerificahubErrorCodes.InsufficientBalance)
{
    Console.WriteLine("Top up your balance");
}
```

A cancelled `CancellationToken` surfaces as the standard `OperationCanceledException`.

## Dependency Injection

The client accepts a caller-provided `HttpClient`, so it integrates with `IHttpClientFactory`.
Register it as **transient** (or scoped) — not singleton: a singleton captures one `HttpClient` for
the application's lifetime, which defeats `IHttpClientFactory`'s handler rotation and the periodic
DNS refresh it provides.

```csharp
services.AddHttpClient("verificahub");

services.AddTransient<IVerificahubClient>(sp =>
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("verificahub");
    return new VerificahubClient(config["Verificahub:ApiKey"]!, config["Verificahub:ApiSecret"]!, httpClient);
});
```

The client never mutates the `HttpClient` you pass — it applies the base address and `Authorization`
header per request — so it is safe to share one across SDKs, and you don't need to set `BaseAddress`
on the named client. It does not dispose an `HttpClient` you supply.

## Development

```bash
dotnet test VerificahubNet.sln
dotnet pack src/VerificahubNet/VerificahubNet.csproj --configuration Release --output artifacts/packages
```

## Documentation

- [Changelog](CHANGELOG.md)

## License

[MIT](LICENSE)
