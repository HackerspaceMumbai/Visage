# Feature Specification: SQL Server Aspire Integration

**Feature Branch**: `001-aspire-sqlserver-integration`  
**Created**: 2025-10-17  
**Status**: Draft  
**Input**: User description: "Presently we are using SQL Server as a standalone. We want to use SQL Server as a 1st class citizen of the Aspire App host and want to both the registration and eventing services to use the SQL Server as their backend."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Centralized SQL Server Resource Management (Priority: P1)

As a DevOps engineer or developer, I need SQL Server to be managed as a first-class Aspire resource so that database lifecycle, connection strings, and health monitoring are handled consistently through the AppHost orchestration rather than manual configuration.

**Why this priority**: This is the foundational change that enables all other benefits. Without Aspire-managed SQL Server, developers must manually manage connection strings, health checks, and service dependencies, leading to configuration drift and deployment issues.

**Independent Test**: Can be fully tested by verifying that SQL Server is registered in AppHost, connection strings are automatically injected into services, and health checks report database status through Aspire dashboard without any manual configuration files.

**Acceptance Scenarios**:

1. **Given** the Aspire AppHost is running, **When** a developer inspects the Aspire dashboard, **Then** SQL Server appears as a registered resource with health status
2. **Given** SQL Server is registered in AppHost, **When** services are started, **Then** connection strings are automatically available to services without manual appsettings.json configuration
3. **Given** SQL Server becomes unavailable, **When** services attempt to connect, **Then** Aspire health checks detect the failure and report it in the dashboard
4. **Given** SQL Server is restarted, **When** it becomes available again, **Then** dependent services automatically reconnect without manual intervention

---

### User Story 2 - Registration Service Database Migration (Priority: P2)

As a backend developer, I need the Registration service to use the Aspire-managed SQL Server for storing registrant data so that all attendee registration information is persisted reliably with proper connection management and observability.

**Why this priority**: Once SQL Server is available as an Aspire resource (P1), the Registration service can migrate its data layer. This service handles critical user data and must be migrated before event operations can benefit from centralized database management.

**Independent Test**: Can be fully tested by creating new registrations, verifying data persists in SQL Server, and confirming that registration queries work correctly through the Aspire-managed connection without any standalone database configuration.

**Acceptance Scenarios**:

1. **Given** a user submits a registration form, **When** the data is saved, **Then** the registrant record appears in the SQL Server database managed by Aspire
2. **Given** existing registrations in the database, **When** the service queries for registrants, **Then** all data is retrieved correctly using the Aspire-injected connection string
3. **Given** the Registration service starts up, **When** it connects to SQL Server, **Then** EF Core migrations run automatically and schema is up-to-date
4. **Given** a registration operation fails due to database constraints, **When** the error occurs, **Then** appropriate error handling returns meaningful messages without exposing connection details

---

### User Story 3 - Eventing Service Database Migration (Priority: P3)

As a backend developer, I need the Eventing service to use the Aspire-managed SQL Server for storing event data so that all meetup event information is centrally managed with consistent data access patterns.

**Why this priority**: After Registration service migration (P2), the Eventing service can move to the shared SQL Server. This completes the database consolidation and ensures both core services use the same infrastructure pattern.

**Independent Test**: Can be fully tested by creating and querying events, verifying data persists in SQL Server, and confirming that event operations work correctly through the Aspire-managed connection.

**Acceptance Scenarios**:

1. **Given** an event organizer creates a new event, **When** the data is saved, **Then** the event record appears in the SQL Server database managed by Aspire
2. **Given** existing events in the database, **When** users query for upcoming events, **Then** all data is retrieved correctly using the Aspire-injected connection string
3. **Given** the Eventing service starts up, **When** it connects to SQL Server, **Then** EF Core migrations run automatically and schema is up-to-date
4. **Given** multiple services accessing the database, **When** they perform operations, **Then** connection pooling ensures efficient resource usage

---

### Edge Cases

- What happens when SQL Server is unavailable during AppHost startup? (Services should wait or fail gracefully with clear error messages)
- How does the system handle connection pool exhaustion under high load? (Should queue requests or return 503 Service Unavailable with retry guidance)
- What happens when EF Core migrations fail during service startup? (Service should not start, log clear migration errors, and report unhealthy status)
- How are connection strings secured in different environments (development vs. production)? (Use Aspire parameter management with environment-specific secrets)
- What happens when two services attempt conflicting schema migrations? (Migration should be coordinated or use separate schemas per service)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST register SQL Server as a resource in the Aspire AppHost with explicit resource naming and lifecycle management
- **FR-002**: System MUST automatically provide connection strings to Registration and Eventing services through Aspire's service discovery without requiring manual configuration
- **FR-003**: System MUST ensure Registration service persists all registrant data to the Aspire-managed SQL Server database
- **FR-004**: System MUST ensure Eventing service persists all event data to the Aspire-managed SQL Server database  
- **FR-005**: System MUST run EF Core migrations automatically on service startup to maintain database schema consistency
- **FR-006**: System MUST implement health checks for SQL Server that report status through Aspire's health monitoring dashboard
- **FR-007**: System MUST declare service startup dependencies using `.WaitFor()` to ensure SQL Server is available before dependent services start
- **FR-008**: System MUST use connection pooling to efficiently manage database connections under concurrent load
- **FR-009**: System MUST handle connection failures gracefully with appropriate retry logic and error messages
- **FR-010**: System MUST secure connection strings using Aspire's parameter management rather than plain-text configuration files

### Key Entities

- **SQL Server Resource**: The database server instance managed by Aspire, with health status, connection information, and lifecycle management
- **Registration Service Database Context**: EF Core DbContext for registrant data, includes entities for attendees, registration status, and profile information
- **Eventing Service Database Context**: EF Core DbContext for event data, includes entities for events, sessions, venues, and schedules
- **Connection Configuration**: Aspire-managed connection strings, including server address, database name, authentication, and connection pooling settings
- **Migration History**: EF Core migration tracking to ensure schema versions are consistent across deployments

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Developers can start the entire Visage solution with a single command and SQL Server is automatically provisioned and connected without manual configuration steps
- **SC-002**: Registration and Eventing services successfully connect to SQL Server within 5 seconds of startup
- **SC-003**: Database health status is visible in the Aspire dashboard and accurately reflects SQL Server availability
- **SC-004**: All database operations for registrations and events complete successfully with data persisting across service restarts
- **SC-005**: Connection string management is simplified, eliminating at least 80% of manual appsettings.json edits previously required for database configuration
- **SC-006**: Service startup order is automatically managed, with zero instances of "database not available" errors during normal startup
- **SC-007**: Database connection pooling handles at least 100 concurrent database operations without connection exhaustion
- **SC-008**: Database schema migrations complete successfully on first service startup in any environment (development, staging, production)

