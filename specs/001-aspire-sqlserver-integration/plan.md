# Implementation Plan: SQL Server Aspire Integration

**Branch**: `001-aspire-sqlserver-integration` | **Date**: 2025-10-17 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-aspire-sqlserver-integration/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Migrate from standalone SQL Server to Aspire-managed SQL Server as a first-class orchestrated resource. The Registration and Eventing services will use the Aspire-managed SQL Server instance with automatic connection string management, health monitoring, and proper service dependency ordering. This eliminates manual configuration, provides centralized observability through the Aspire dashboard, and ensures consistent database access patterns across services.

## Technical Context

**Language/Version**: C# 14 / .NET 10  
**Primary Dependencies**: .NET Aspire 10, EF Core 10, Microsoft.Data.SqlClient, Aspire.Hosting.SqlServer  
**Storage**: SQL Server (Aspire-managed resource)  
**Testing**: TUnit, Fluent Assertions, Aspire.Testing for integration tests  
**Target Platform**: Cross-platform (.NET 10 runtime - Windows/Linux/macOS)
**Project Type**: Distributed services (Aspire-orchestrated microservices)  
**Performance Goals**: Database connection within 5 seconds, support 100+ concurrent operations, migration completion < 30 seconds  
**Constraints**: Must use Aspire hosting patterns, EF Core migrations, connection pooling, health checks required  
**Scale/Scope**: 2 services (Registration, Eventing), shared SQL Server resource, existing entities migrate to new infrastructure

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Aspire-First Architecture**: SQL Server registered in AppHost with `.WaitFor()` dependencies for Registration and Eventing services
- [x] **Minimal API Design**: No new API endpoints (infrastructure change only), existing APIs unchanged
- [x] **Integration Testing Priority**: Integration tests planned for database connectivity, EF Core migrations, health checks
- [x] **Blazor Hybrid UI**: N/A - This is backend infrastructure change only
- [x] **Observability**: SQL Server health checks will report to Aspire dashboard, OpenTelemetry already enabled via ServiceDefaults
- [x] **Security & Privacy**: Connection strings secured via Aspire parameters, no Auth0 changes needed (infrastructure only)
- [x] **Technology Showcase**: Uses latest .NET 10/Aspire features for SQL Server hosting and service orchestration

*All checks pass - No violations to justify.*

## Project Structure

### Documentation (this feature)

```text
specs/001-aspire-sqlserver-integration/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (N/A - no new API contracts)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Visage.AppHost/
├── Program.cs                           # MODIFIED: Add SQL Server resource registration
└── appsettings.json                     # MODIFIED: Add SQL Server connection parameters

services/Visage.Services.Registrations/
├── Program.cs                           # MODIFIED: Update to use Aspire-injected connection
├── RegistrantDB.cs                      # MODIFIED: Remove hardcoded connection string
├── appsettings.json                     # MODIFIED: Remove connection string (now from Aspire)
└── Migrations/                          # EXISTING: EF Core migrations (unchanged)

services/Visage.Services.Eventing/
├── Program.cs                           # MODIFIED: Update to use Aspire-injected connection
├── EventDB.cs                           # MODIFIED: Remove hardcoded connection string
├── appsettings.json                     # MODIFIED: Remove connection string (now from Aspire)
└── Migrations/                          # EXISTING: EF Core migrations (unchanged)

tests/Visage.Tests.Integration/
├── SqlServerIntegrationTests.cs         # NEW: Tests for Aspire SQL Server integration
├── RegistrationDbTests.cs               # MODIFIED: Update for Aspire test host
└── EventingDbTests.cs                   # MODIFIED: Update for Aspire test host

Visage.ServiceDefaults/
└── Extensions.cs                        # EXISTING: Already provides health checks (no changes)
```

**Structure Decision**: This is an infrastructure migration affecting existing services. We modify the AppHost to register SQL Server as an Aspire resource, update both Registration and Eventing services to consume the Aspire-managed connection string, and add integration tests using Aspire.Testing to validate the configuration.

## Complexity Tracking

No constitution violations - this section is not applicable for this feature.


