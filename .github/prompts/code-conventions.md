# Code Conventions

## General Guidelines

- Use the latest .NET 9 features and libraries
- Follow official .NET 9 coding guidelines as provided by Microsoft
- Ensure code is clean, readable, and maintainable
- Follow the DRY (Don't Repeat Yourself) principle
- Follow the KISS (Keep It Simple, Stupid) principle
- Split large files and long functions into smaller, more focused units
- Use descriptive variable names with auxiliary verbs (e.g., IsLoading, HasError)
- Commit messages should be in imperative form, sentence case, starting with a verb, with NO trailing dot
- All git commit messages should be signed off by the author
- AVOID making changes to code unrelated to the current task

## Project-Specific Conventions

- Use C# top-level namespaces
- Minimal API endpoints are always contained in a single file
- Use EF Core for data access
- Follow the Repository pattern for data access
- Implement the Unit of Work pattern for data access
- Use Scalar OpenAPI for API documentation
- Ensure services in Aspire AppHost are properly configured to wait for dependencies