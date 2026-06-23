# VerificahubNet

.NET SDK for the [Verificahub](https://verificahub.ru) phone-verification API — initiate
verifications (SMS, flash call, reverse flash call, Telegram), check codes, poll status, read balance
and usage, and validate webhook signatures.

## Install

```bash
dotnet add package VerificahubNet
```

Multi-targets `netstandard2.0`, `net8.0`, and `net10.0`.

## Quick Start

```csharp
using VerificahubNet;
using VerificahubNet.Models;
using VerificahubNet.Requests;

using var client = new VerificahubClient("vh_live_...", "vh_sec_...");

var verification = await client.VerifyAsync(new InitiateVerificationRequest("+79991234567"));

if (verification is ReverseFlashCallInitiateResponse flash)
    Console.WriteLine($"Ask the user to dial {flash.NumberToCall}");

// Later, poll for completion:
var status = await client.GetStatusAsync(verification.RequestId);
bool done = status.Status == VerificationStatus.Verified;
```

## Supported API

| Endpoint | Client API |
| --- | --- |
| `POST /v1/verify` | `VerifyAsync` |
| `GET /v1/verify/{request_id}` | `GetStatusAsync` |
| `POST /v1/verify/check` | `CheckCodeAsync` |
| `GET /v1/balance` | `GetBalanceAsync` |
| `GET /v1/usage` | `GetUsageAsync` |

It also includes a `WebhookSignatureValidator` for verifying the `X-Verificahub-Signature` header on
`verification.*` webhook deliveries.

## Error Handling

Non-2xx responses throw `VerificahubApiException`; branch on its `ErrorCode` (see
`VerificahubErrorCodes`). Transport failures throw `VerificahubTransportException`, and unreadable
responses throw `VerificahubProtocolException`.

```csharp
try
{
    var result = await client.CheckCodeAsync(new CheckCodeRequest(requestId, code));
}
catch (VerificahubApiException ex) when (ex.ErrorCode == VerificahubErrorCodes.InvalidCode)
{
    Console.WriteLine($"Wrong code, {ex.AttemptsRemaining} attempts left");
}
```

## Repository

Source, issue tracking, and full documentation:

https://github.com/verificahub/verificahub-sdk-net
