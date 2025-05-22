# .NET Aspire Guidance

## Aspire Architecture

Visage uses .NET Aspire for orchestrating services and resources. When working with Aspire components:

- Add all services as Project Resources in the Aspire AppHost
- Configure service dependencies to ensure proper startup ordering
- Leverage Aspire's built-in service discovery
- Use the appropriate Aspire components for common resources (databases, caches, etc.)

## Aspire Resources

When adding or modifying Aspire resources:

- Use dependency injection for resource configuration
- Apply appropriate resilience patterns
- Configure health checks for all resources
- Ensure proper telemetry is set up for observability
- Use standardized configuration approaches for connection strings and settings

## Testing Aspire Services

For testing Aspire-orchestrated services:

- Leverage the Aspire testing framework for integration tests
- Mock external dependencies appropriately
- Ensure tests run in isolation
- Validate configuration before runtime
- Test service discovery mechanisms