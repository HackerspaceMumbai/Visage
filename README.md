<div align="center">    
<h1>Visage</h1>
Meetups done right      
</div>

<hr1>

[![PRs Welcome][prs-badge]][prs]

## The problem
We are the largest Open Source Software[OSS] community with its longest running(> 10 years) tech meetup in Bombay. Most of our events are houseful.

1. We have to curate the registrants based on the theme and also to ensure we are inclusive, diverse, and have a good mix of attendees.
2. We have to ensure that the attendees are checked-in to the event and also to the individual sessions. This is important for us to understand the attendee interest and also to ensure that we are able to provide the best experience to the attendees. Most importantly, we need to record the checkout time so as to comply with building/civic code.
3. With [DPDP](https://www.meity.gov.in/writereaddata/files/Digital%20Personal%20Data%20Protection%20Act%202023.pdf) (India's equivalent of GDPR) coming into effect, we have to ensure that the data we collected is secure, is not misused, and resides within the jurisdiction of India.

## The solution

We will build a .NET solution that takes full advantage of AI and Azure services to solve the problem. The solution will have the following components:

1. Aspire
2. Semantic Kernel
3. Blazor Universal
4. Azure Open AI

For further reading on architecture, please check our website [here](https://hackmum.in)

We are targeting to launch the product on [12th November to coincide with .NET Conf 2024 launch](https://www.meetup.com/mumbai-technology-meetup/events/303879655/?eventOrigin=group_upcoming_events).

## Testing Guidelines

We have integrated a comprehensive test suite into the Visage project to ensure the quality and reliability of our codebase. The test suite includes unit tests, integration tests, and end-to-end tests. Below are the guidelines and instructions for running the tests.

### Prerequisites

Ensure you have the following tools installed:

- .NET SDK
- Node.js (for Playwright)
- Playwright CLI

### Running Unit Tests

Unit tests are written using TUnit and FluentAssertions. To run the unit tests, execute the following command:

```bash
dotnet test tests/Visage.Tests.Unit/Visage.Tests.Unit.csproj
```

### Running Integration Tests

Integration tests are also written using TUnit and FluentAssertions. To run the integration tests, execute the following command:

```bash
dotnet test tests/Visage.Tests.Integration/Visage.Tests.Integration.csproj
```

### Running End-to-End Tests

End-to-end tests are written using Playwright. To run the end-to-end tests, execute the following command:

```bash
npx playwright test
```

### Configurable Tests

Our tests are configurable to run on different endpoints for local workstations and CI/CD environments. Ensure you have the appropriate configuration set in your environment variables.

### Writing New Tests

When writing new tests, follow these guidelines:

- Use TUnit and FluentAssertions for unit and integration tests.
- Use Playwright for end-to-end tests.
- Ensure tests are clear, maintainable, and cover critical functionalities.
- Document any new tests added to the suite.

For more detailed instructions and examples, refer to the test project files in the `tests` directory.

By following these guidelines, we can ensure that our codebase remains reliable, maintainable, and of high quality.
