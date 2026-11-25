<table>
  <tr>
    <td width="170" align="center" valign="middle">
      <img src="assets/icon.png" width="140" alt="VerificahubNet logo" />
    </td>
    <td valign="middle">
      <h1>VerificahubNet</h1>
      <p>.NET SDK for the <a href="https://verificahub.ru">Verificahub</a> phone-verification API — reverse flash call, flash call, SMS, voice call, Telegram, WhatsApp, MTS ID and MAX, with typed models and webhook signature validation.</p>
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
status, read balance, prices and usage — plus webhook signature validation, and follows Semantic Versioning.

> **Methods:** `reverse_flash_call`, `flash_call`, `sms`, `pass_call`, `telegram_otp`, `whatsapp_otp`,
> `mts_id` and `max_bot`. **Region:** Russia (`+7`).

## Installation

```bash
dotnet add package VerificahubNet
```

The library multi-targets `netstandard2.0`, `net8.0`, and `net10.0`, so it runs on .NET Framework
4.6.1+, .NET Core 2.0+, Mono, Unity, and modern .NET.

## Before you start

Obtain an `api_key` and `api_secret` from your Verificahub dashboard. The API opens once your project
has passed **project verification** in the dashboard, and then only from the server addresses approved
for it. Until then every call fails with `compliance_required` or `ip_not_allowed` — see
[Error Handling](#error-handling).

## Quick Start

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

`VerifyAsync` returns a subtype of `InitiateVerificationResponse` chosen by the method:

| Method | Response type | What the user does |
| --- | --- | --- |
| `ReverseFlashCall` *(default)* | `ReverseFlashCallInitiateResponse` (`NumberToCall`) | Dials the number; no code |
| `FlashCall` | `FlashCallInitiateResponse` | Types the code from the caller ID |
| `Sms` | `SmsInitiateResponse` | Types the code from the SMS |
| `PassCall` | `PassCallInitiateResponse` | Answers the call, types the dictated code |
| `TelegramOtp` | `TelegramInitiateResponse` | Types the code from Telegram |
| `WhatsappOtp` | `WhatsAppInitiateResponse` | Types the code from WhatsApp |
| `MtsId` | `MtsIdInitiateResponse` | Confirms on the SIM, or types a code on fallback |
| `MaxBot` | `MaxBotInitiateResponse` (`DeepLink`) | Opens the MAX bot, types the code |

A method the API adds after this SDK version was built comes back as `UnknownInitiateResponse`
(with `RawMethod`) instead of throwing, so you never lose the `RequestId` of a verification that has
already started.

`reverse_flash_call` completes automatically — the user dials a number we hand out and we verify by
caller id, so there is no code to submit. Poll for completion:

```csharp
var status = await client.GetStatusAsync(verification.RequestId);

bool done = status.Status == VerificationStatus.Verified;
```

The code-based methods deliver a code the user enters; submit it with `CheckCodeAsync`:

```csharp
var result = await client.CheckCodeAsync(new CheckCodeRequest(verification.RequestId, "1234"));
bool verified = result.Status == VerificationStatus.Verified;
```

### When a verification does not succeed

`GetStatusAsync` reports `FailureReason` once a session ends `Expired` or `Failed`. Use it to decide
what to offer next:

```csharp
switch (status.FailureReason)
{
    case VerificationFailureReason.NotDelivered:
    case VerificationFailureReason.Unreachable:
        // The channel could not reach the user — offer a different method.
        break;
    case VerificationFailureReason.Declined:
        // The user refused; retrying the same channel rarely helps.
        break;
}
```

### Voice call (`pass_call`)

A robot calls the number and dictates the code. It is charged when the call is **requested**, not
when it is answered, and the call shows the caller-ID label of the platform's legal entity rather than
your service name — tell the user who will be calling.

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

`max_bot` returns a MAX deep link (`MaxBotInitiateResponse.DeepLink`) to show as a link or QR. The user
opens the bot, shares their number, and the bot delivers the code. Poll `AwaitingCode` (or handle the
`verification.delivered` webhook) to know when it arrived, then submit it with `CheckCodeAsync`:

```csharp
if (verification is MaxBotInitiateResponse max)
    Console.WriteLine($"Open the MAX bot: {max.DeepLink}");
```

## Balance, prices and usage

```csharp
var balance = await client.GetBalanceAsync();
// Check Available, not Balance: an account with an agreed credit limit may spend below zero, and a
// request costing more than Available is refused with insufficient_balance.
Console.WriteLine($"{balance.Available.Amount} {balance.Available.Currency} left to spend");

var prices = await client.GetPricesAsync();
foreach (var p in prices.Prices.Where(p => p.Available))
    Console.WriteLine($"{p.Method}: {p.Price.Amount} {p.Price.Currency}");

var usage = await client.GetUsageAsync(from: DateTime.UtcNow.AddDays(-7), to: DateTime.UtcNow);
Console.WriteLine($"{usage.Successful}/{usage.TotalVerifications} verified");
```

`GetPricesAsync` lists every priced method; `Available` says whether your account may request it.
Calling `VerifyAsync` with one that is not available fails with `method_not_available`.

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
  `AttemptsRemaining` are available, and `Problem` carries any extra fields.
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

Two `403`s mean the credentials are right but the project or the calling server is not allowed yet.
Handle them separately from `401`:

| `ErrorCode` | Meaning | Extra fields on `ex.Problem` |
| --- | --- | --- |
| `ComplianceRequired` | The project has not passed verification in the dashboard. | `ComplianceStatus`, `DashboardUrl` |
| `IpNotAllowed` | The request came from an address not approved for the project. | `SourceIp` |
| `MethodNotAvailable` | The method is not enabled for your account. | — |

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

## Upgrading from 1.x

2.0 tracks what the API serves today. See the [changelog](CHANGELOG.md) for the full list; the
breaking parts are:

- Number lookups are gone (`LookupNumberInfoAsync`, `LookupActivityAsync`, and the `Lookups` members of
  the prices and usage responses) — the API no longer offers them.
- `VerificationMethod.MaxOtp` and `VerificationMethod.Voice` are gone. Use `MaxBot` for MAX and
  `PassCall` for a voice call.
- An unrecognised `method` on `VerifyAsync` now yields `UnknownInitiateResponse` instead of a
  `VerificahubProtocolException`.

## Development

```bash
dotnet test VerificahubNet.sln
dotnet pack src/VerificahubNet/VerificahubNet.csproj --configuration Release --output artifacts/packages
```

## Documentation

- [Changelog](CHANGELOG.md)

## License

[MIT](LICENSE)
