# Minimal API Guidance

## API Design Principles

When designing and implementing Minimal APIs in Visage:

- Keep each API endpoint in a single file
- Follow RESTful principles for resource naming and operations
- Use appropriate HTTP methods and status codes
- Implement proper validation using FluentValidation
- Structure endpoints for clear separation of concerns
- Utilize endpoint filters for cross-cutting concerns

## Route Organization

- Group related endpoints logically
- Use consistent naming conventions for routes
- Implement versioning when appropriate
- Keep route parameters simple and clear
- Use query parameters for filtering, sorting, and pagination

## Documentation

- Use Scalar OpenAPI for comprehensive API documentation
- Include examples and descriptions for each endpoint
- Document request and response models thoroughly
- Provide clear error response documentation
- Keep documentation synchronized with implementation

## Implementation Practices

- Use dependency injection for services
- Implement proper error handling and return appropriate status codes
- Return consistent response formats
- Use cancellation tokens for long-running operations
- Implement proper logging for API operations
- Consider rate limiting for public-facing APIs
- Test all endpoints with both valid and invalid inputs