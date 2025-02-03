
# Copilot Instructions

Visage is an app to manage events for OSS community events promoting inclusiveness and diversity.

It has the following tech architecture:

    1. A Hybrid Blazor Web App Front End: Please ask clarifying questions to ask if this feature is targeted for Organizers or attendees. Apart from the home page which will be rendered by default SSR , all  razor pages will use the auto render mode.
    2. Please ask a clarifying questions if the Blazor components need to be rendered in the MAUI part 
    3. The solution is orchestrated through the .NET Aspire workload for local developer loop.
    4. Each service is a Minimal API that will have its own .http file to do some ad-hoc testing
    5. The Minimal API will connect to the backend via EF Core.

## General Coding Guidelines

### Adhere to .NET 9 Best Practices:

Use the latest .NET 9 features and libraries.
Follow official .NET 9 coding guidelines as provided by Microsoft.
Ensure code is clean, readable, and maintainable.

### Backend Guidelines

    • Utilize .NET 9 features, including Aspire for project orchestration.
    • When implementing new code, closely examine the coding conventions used in this project:
        ○ Use C# top-level namespaces.
        ○ Minimal API endpoints are always only one file.
        ○ Use EF Core for data access.
        ○ Use the Repository pattern for data access.
        ○ Use the Unit of Work pattern for data access.
        ○ Use Scalar OpenAPI for API documentation.

## General Practices
    • AVOID making changes to code that are not related to the change we are doing. E.g., don't remove comments or types.
    • Assume all code is working as intended, as everything has been carefully crafted.
    • Commit messages should be in imperative form, sentence case, starting with a verb, and have NO trailing dot.
    • Use descriptive variable names with auxiliary verbs (e.g., IsLoading, HasError).
    • Ensure all git commit messages are appended by "Signed-off-by: Author Name <authoremail@example.com>" 
    • Ensure all code is well-documented and follows the .NET 9 coding standards.
