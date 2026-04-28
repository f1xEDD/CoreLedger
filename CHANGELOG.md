# Changelog

## Unreleased

### Added
- Added transfer outbox message creation with event type `coreledger.transfer.created.v1`.

### Tests
- Added transfer service coverage for outbox message creation and payload shape.
- Added outbox dispatcher coverage for retryable and terminal publish failures.

### Docs
- Documented transfer outbox event and dispatcher retry behavior in the README.

## v0.1.0 - 2026-04-28

### Added
- Added `POST /accounts`.
- Added `POST /transfers` with `Idempotency-Key` support.
- Added `GET /transfers/{id}`.
- Added `GET /accounts/{id}/balance`.
- Added Result-based error mapping.
- Added PostgreSQL integration tests with Testcontainers.
- Added health/readiness endpoints.
- Added OpenAPI/Scalar API reference for local development.

### Changed
- Introduced clean layering: Domain, Application, Infrastructure, Api.
- Switched expected application failures to `Result<T>` and `AppError`.

### Docs
- Added README with local run instructions and API examples.
