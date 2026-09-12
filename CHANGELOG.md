# Changelog

All notable changes to this project are documented here. The format is based on
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
