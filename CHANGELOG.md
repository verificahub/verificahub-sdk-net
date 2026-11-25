# Changelog

All notable changes to this project are documented here. The format is based on
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.0.0]

Brings the client back in line with the API. Several things 1.x offered had stopped working: two
methods could not be requested at all, and the lookup calls were answered with `404`.

### Added

- **WhatsApp** — `VerificationMethod.WhatsappOtp` (`whatsapp_otp`) and `WhatsAppInitiateResponse`. The
  code arrives from the official VerificaHub WhatsApp Business account and is submitted with
  `CheckCodeAsync`.
- **Voice call** — `VerificationMethod.PassCall` (`pass_call`) and `PassCallInitiateResponse`. A robot
  calls and dictates the code. Charged when the call is requested, not when it is answered.
- `GetVerificationStatusResponse.FailureReason` — why a session ended `Expired` or `Failed`
  (`VerificationFailureReason`: `NoResponse`, `Declined`, `WrongCode`, `NotDelivered`, `Unreachable`,
  `Unknown`). `null` while the session is open or once it is verified.
- `MethodPriceResponse.Available` — whether your account may request the method. Every priced method is
  listed; requesting one marked unavailable fails with `method_not_available`.
- Error codes `ComplianceRequired`, `IpNotAllowed`, `MethodNotAvailable`, `SmsDestinationNotAllowed`
  and `Forbidden`, with `ProblemDetails.ComplianceStatus`, `.DashboardUrl` and `.SourceIp` for the first
  two. Both `compliance_required` and `ip_not_allowed` are `403`, not `401`: the credentials are fine,
  the project or the calling server is not yet allowed.
- `UnknownInitiateResponse` — see *Changed*.

### Changed

- **`VerifyAsync` no longer throws on a method it does not recognise.** It returns
  `UnknownInitiateResponse` with the common fields and the raw `method` in `RawMethod`. By the time the
  response is read the verification has already started — and may have been charged — so throwing lost
  its `RequestId`. This is what happened to every 1.x caller that requested `whatsapp_otp` or `pass_call`.
- `VerificationMethod` members now have explicit values. Members that remain keep their 1.x numbers, so
  integers you have persisted keep their meaning.
- `VerificahubErrorCodes.NoGatewayAvailable` is `[Obsolete]`: the API no longer returns it.

### Removed

- **Number lookups.** `LookupNumberInfoAsync`, `LookupActivityAsync`, `LookupRequest`, `LookupResponse`,
  `LookupMethod`, `LookupStatus`, `MobileOperator`, `GetPricesResponse.Lookups`,
  `LookupPriceResponse`, `GetUsageResponse.Lookups` and `LookupUsageResponse`. The API no longer offers
  lookups; the calls were returning `404`.
- `VerificationMethod.MaxOtp` — withdrawn from the public API. Use `MaxBot`.
- `VerificationMethod.Voice` — the API never accepted it. The voice method is `PassCall`.

## [1.1.0]

Catches the client up with the API: one verification method and a whole service had shipped without it.

### Added

- **MAX** verification method — `VerificationMethod.MaxOtp` (`max_otp`). The code is sent straight to
  the user's number as a MAX message: nothing to open, no bot, no deep link, and the code comes back
  through `CheckCodeAsync` as with SMS. Distinct from `MaxBot`, which is cheaper but costs the user an
  extra step; they are different methods at different prices and the names invite confusion.
- **Number lookups** — `LookupNumberInfoAsync` (`POST /v1/lookup/number-info`) and `LookupActivityAsync`
  (`POST /v1/lookup/activity`), with `LookupRequest`, `LookupResponse`, `LookupMethod`, `LookupStatus`
  and `MobileOperator`.

  A lookup is not a verification — it reads the operator's registry and proves nothing about who holds
  the phone. **"No data" is an answer, not an error:** `LookupStatus.NoData` arrives as a `200` with a
  zero `Cost`, retrying will not change it, and a number with no activity score may still answer
  number-info. Reconcile against `Cost` rather than against the requests you made.
- `GetPricesResponse.Lookups` — the lookup tariffs, listed separately from the verification methods
  because they are charged per answer rather than at a delivery milestone.
- `GetUsageResponse.Lookups` — lookup usage counted separately from verifications, with `Billable`
  below `Total` by exactly the requests that found nothing. Spend for a range is the two totals added
  together.
- `GetBalanceResponse.CreditLimit` and `.Available` — for an account with an agreed credit limit the
  balance may go below zero. **Check `Available`, not `Balance`:** a request costing more than
  `Available` is refused with `insufficient_balance`, and for a credited account the two differ.

### Note

Unknown enum values still fall back to `Unknown` rather than throwing, so a method or lookup service
added server-side does not break a client that has not been upgraded.


## [1.0.0]

First stable release. Covers every `/v1/*` external endpoint and every verification method the API offers.

### Added

- **MAX bot** verification method. `VerificationMethod.MaxBot` (`max_bot`) and the
  `MaxBotInitiateResponse` shape returned by `VerifyAsync`, carrying `DeepLink` (`deep_link`) — the MAX
  deep link you show the user. They open the bot and share their number, the bot delivers the code in the
  MAX messenger, and the user submits it via `CheckCodeAsync`. The status starts `Sent` and flips to
  `Delivered` (also surfaced by `awaiting_code`) once the code reaches the user.
- `GetPricesAsync` (`GET /v1/prices`) with `GetPricesResponse` / `MethodPriceResponse` — the live
  per-method prices for your account currency.

## [0.2.0-preview.1]

### Added

- **MTS ID** verification method. `VerificationMethod.MtsId` (`mts_id`) and the
  `MtsIdInitiateResponse` shape returned by `VerifyAsync`. MTS confirms silently on the SIM and the
  result arrives over the notification webhook, so the initiate status starts `Sent`; on SMS-OTP
  fallback a code is expected and submitted via `CheckCodeAsync`.
- `GetVerificationStatusResponse.AwaitingCode` (`awaiting_code`) — `true` once an `mts_id` flow has
  fallen back to SMS-OTP and is waiting for the user's code; poll it to know when to prompt.
- `WebhookEventTypes.VerificationCodeRequired` (`verification.code_required`) — fired when an
  `mts_id` flow falls back to SMS-OTP and a typed code becomes expected.

These are additive; existing code continues to compile and behave unchanged.

## [0.1.0-preview.1]

### Added

- Initial preview of `VerificahubClient` covering the external integrator API (`/v1/*`):
  `VerifyAsync`, `GetStatusAsync`, `CheckCodeAsync`, `GetBalanceAsync`, `GetUsageAsync`.
- HTTP Basic authentication with the account's `api_key` / `api_secret`.
- Typed exception hierarchy (`VerificahubApiException`, `VerificahubTransportException`,
  `VerificahubProtocolException`) mapping RFC-7807 problem-details responses, including
  `error_code`, `trace_id`, and `attempts_remaining`.
- `WebhookSignatureValidator` for verifying the `X-Verificahub-Signature` header on
  `verification.*` webhook deliveries, plus a typed `WebhookEvent` payload.
