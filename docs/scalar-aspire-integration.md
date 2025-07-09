# Scalar Aspire Integration

This document describes how Scalar API documentation is integrated with .NET Aspire in the Visage project, following the official Scalar guide.

## Overview

Scalar provides interactive API documentation for .NET APIs. This integration follows the official approach recommended in the [Scalar .NET Aspire Integration Guide](https://guides.scalar.com/scalar/scalar-api-references/integrations/net-aspire).

## Implementation

### Services Configuration

Each API service configures Scalar directly in their `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components
builder.AddServiceDefaults();

// Add OpenAPI services
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure Scalar in development
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("My API")
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}
```

**Required Package Reference:**
```xml
<PackageReference Include="Scalar.AspNetCore" Version="2.4.13" />
```

### AppHost Configuration

The AppHost configures dashboard URLs to link directly to Scalar endpoints:

```csharp
var eventAPI = builder.AddProject<Projects.EventService>("event-api")
    .WithUrlForEndpoint("http", url => 
        url.DisplayLocation = UrlDisplayLocation.DetailsOnly) // Hide HTTP link
    .WithUrlForEndpoint("https", url =>
    {
        url.DisplayText = "Event API Scalar OpenAPI";
        url.Url += "/scalar/v1";  // Default Scalar endpoint
    });
```

## Current Services

### Event API
- **Service**: Uses `MapScalarApiReference` with title "Visage Event API"
- **AppHost**: Configured with "Event API Scalar OpenAPI" dashboard link
- **URL**: `https://localhost:{port}/scalar/v1`

### Registration API  
- **Service**: Uses `MapScalarApiReference` with title "Visage Registration API"
- **AppHost**: Configured with "Registration API Scalar OpenAPI" dashboard link
- **URL**: `https://localhost:{port}/scalar/v1`

## Benefits

- **Development-only**: Scalar UI only appears in development environment
- **Direct Integration**: Services configure Scalar directly following official patterns
- **Dashboard Links**: Aspire dashboard provides direct access to API documentation
- **Standard Endpoints**: Uses default `/scalar/v1` path for consistency