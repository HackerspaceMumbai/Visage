# API Contracts

**Feature**: SQL Server Aspire Integration

## Status

**N/A - No new API contracts for this feature**

This is an infrastructure migration that changes how services connect to SQL Server. No new API endpoints are added, and existing API contracts remain unchanged.

## Existing APIs

The following existing APIs continue to work without modification (internal implementation changes only):

### Registration Service

- All existing registration endpoints (unchanged)
- Connection to database now via Aspire-managed connection string
- No breaking changes to API contracts

### Eventing Service

- All existing eventing endpoints (unchanged)
- Connection to database now via Aspire-managed connection string
- No breaking changes to API contracts

## Health Endpoints

Both services expose health endpoints (already existing):

- `GET /health` - Overall service health (includes database connectivity check)
- `GET /alive` - Liveness check

These endpoints are automatically configured by `ServiceDefaults` and are unchanged by this feature.
