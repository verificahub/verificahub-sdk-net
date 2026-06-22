# Changelog

All notable changes to this project are documented here. The format is based on
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
