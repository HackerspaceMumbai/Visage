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

## Project Architecture and Components

The Visage project is built using a modular architecture to ensure scalability, maintainability, and ease of development. Below is an overview of the project's architecture and its main components:

### 1. Aspire

Aspire is the core framework that powers the Visage project. It provides the foundation for building scalable and reliable applications. Aspire includes features such as dependency injection, configuration management, logging, and more.

### 2. Semantic Kernel

The Semantic Kernel is responsible for handling natural language processing (NLP) tasks within the Visage project. It leverages advanced machine learning models to understand and process user inputs, enabling intelligent interactions and responses.

### 3. Blazor Universal

Blazor Universal is the front-end framework used in the Visage project. It allows for building interactive web applications using C# and .NET. Blazor Universal provides a seamless development experience by enabling code sharing between the client and server.

### 4. Azure Open AI

Azure Open AI is integrated into the Visage project to leverage the power of artificial intelligence. It provides advanced AI capabilities, such as natural language understanding, image recognition, and predictive analytics. Azure Open AI enables the Visage project to deliver intelligent and personalized experiences to users.

## Deployment Process

The deployment process for the Visage project involves the following steps:

1. **Build**: The project is built using the .NET SDK. This step compiles the source code and generates the necessary artifacts for deployment.

2. **Containerization**: The project is containerized using Docker. Docker images are created for each component of the Visage project, ensuring consistency and portability across different environments.

3. **Infrastructure Provisioning**: The necessary infrastructure is provisioned using Azure services. This includes setting up virtual machines, databases, storage accounts, and other resources required for the Visage project.

4. **Deployment**: The Docker images are deployed to the provisioned infrastructure using Azure Kubernetes Service (AKS). AKS provides a scalable and managed environment for running containerized applications.

5. **Configuration**: The deployed components are configured with the necessary environment variables, secrets, and connection strings. This ensures that the Visage project can connect to the required services and operate correctly.

6. **Monitoring and Logging**: The deployed components are monitored using Azure Monitor and Application Insights. This allows for real-time monitoring of the application's performance, availability, and health. Logs are collected and analyzed to identify and troubleshoot any issues.

7. **Scaling**: The Visage project is designed to scale horizontally to handle increased traffic and workload. Azure Kubernetes Service (AKS) automatically scales the deployed components based on the defined scaling policies.

By following this deployment process, the Visage project can be reliably deployed and maintained in a production environment.

## Further Reading

For more detailed information about the Visage project, please refer to the following resources:

- [Project Wiki](https://github.com/HackerspaceMumbai/Visage/wiki)
- [Project Website](https://hackmum.in)

These resources provide additional documentation, tutorials, and guides to help you get started with the Visage project and understand its architecture and components.
