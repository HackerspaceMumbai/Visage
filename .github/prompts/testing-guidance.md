# Testing Guidance

## Testing Philosophy

Visage aims for comprehensive test coverage using a combination of testing approaches:

- **Integration Testing**: Primary testing method using TUnit, Fluent Assertions, & Playwright
- **Unit Testing**: For isolated business logic components
- **Load Testing**: Using NBomber for performance validation
- **Security Testing**: Using OWASP ZAP for security validation
- **Chaos Testing**: Using Stryker for resilience validation

## Testing Standards

- Strive for 100% test coverage
- External connections should be mocked via NSubstitute
- Tests should be deterministic and not depend on external services
- Each test should validate a single behavior or requirement
- Tests should be fast, isolated, repeatable, and thorough

## Writing Tests

When writing tests for Visage:

- Use descriptive test names that explain the expected behavior
- Follow the Arrange-Act-Assert pattern
- Use Fluent Assertions for readable assertions
- Mock external dependencies appropriately
- Consider edge cases and error conditions
- Write both positive and negative test cases
- Ensure tests run in isolation

## Testing Aspire Components

For testing .NET Aspire components:

- Use the Aspire testing framework
- Test service discovery and dependency management
- Validate configuration and environment variables
- Test resilience patterns and error handling
- Ensure proper resource cleanup after tests