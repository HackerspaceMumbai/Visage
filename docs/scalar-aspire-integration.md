# Scalar Aspire Integration

This document describes how to use Scalar API documentation with .NET Aspire in the Visage project.

## Overview

Scalar has been integrated as an Aspire primitive, providing a simplified way to add interactive API documentation to your Aspire applications. The integration includes both service-level configuration and AppHost orchestration support.

## Features

- **Centralized Configuration**: Scalar setup is included in the default service configuration
- **Aspire Integration**: Easy configuration through the AppHost with the `WithScalarApiDocumentation()` extension method
- **Development Mode**: Automatically enabled only in development environment
- **Customizable**: Support for custom titles, paths, and configurations

## Usage

### In Services (Automatic)

Services that use `builder.AddServiceDefaults()` automatically get Scalar support. Simply call:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add service defaults (includes Scalar configuration)
builder.AddServiceDefaults();

var app = builder.Build();

// Map Scalar with default configuration
app.MapScalarDefaults("My API");

// Or with custom configuration
app.MapScalarDefaults(options =>
{
    options.WithTitle("My Custom API")
           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.Run();
```

### In AppHost

Configure Scalar endpoints for your services in the AppHost:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add project with Scalar documentation
var eventAPI = builder.AddProject<Projects.MyEventAPI>("event-api")
    .WithScalarApiDocumentation("Event API");

// With custom path
var registrationAPI = builder.AddProject<Projects.MyRegistrationAPI>("registration-api")
    .WithScalarApiDocumentation("Registration API", "/docs/v1");

builder.Build().Run();
```

## API Reference

### ServiceDefaults Extensions

#### `AddScalarDefaults()`
Adds Scalar support to service defaults, including OpenAPI services.

#### `MapScalarDefaults(title, path)`
Maps Scalar API reference endpoints with default configuration.

**Parameters:**
- `title` (optional): The title for the API documentation
- `path` (optional): The path where Scalar UI will be served (defaults to "/scalar/v1")

### AppHost Extensions

#### `WithScalarApiDocumentation(title, scalarPath)`
Configures a project resource to expose Scalar API documentation.

**Parameters:**
- `title` (optional): The title for the API documentation
- `scalarPath` (optional): The path where Scalar UI will be served (defaults to "/scalar/v1")

## Examples

### Basic Service Setup

```csharp
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components (includes Scalar)
builder.AddServiceDefaults();

builder.Services.AddDbContext<MyDB>(opt => opt.UseInMemoryDatabase("MyList"));

var app = builder.Build();

// Configure Scalar with service name
app.MapScalarDefaults("My Service API");

app.UseHttpsRedirection();

// Your API endpoints
var api = app.MapGroup("/api");
api.MapGet("/items", GetItems).WithOpenApi();

app.Run();
```

### AppHost Configuration

```csharp
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Services with Scalar documentation
var eventAPI = builder.AddProject<Projects.EventService>("event-api")
    .WithScalarApiDocumentation("Event Management API");

var userAPI = builder.AddProject<Projects.UserService>("user-api")
    .WithScalarApiDocumentation("User Management API", "/api-docs");

// Frontend references the APIs
var webapp = builder.AddProject<Projects.WebApp>("webapp")
    .WithReference(eventAPI)
    .WithReference(userAPI);

builder.Build().Run();
```

## Migration Guide

If you're migrating from manual Scalar configuration:

### Before
```csharp
// Manual configuration
builder.Services.AddOpenApi();

var app = builder.Build();

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

### After
```csharp
// Using Scalar Aspire primitive
builder.AddServiceDefaults(); // Includes OpenAPI and Scalar setup

var app = builder.Build();

app.MapScalarDefaults("My API"); // Handles development check automatically
```

## Troubleshooting

### Scalar UI not appearing
- Ensure you're running in Development environment
- Check that `AddServiceDefaults()` is called before building the app
- Verify that `MapScalarDefaults()` is called after building the app

### OpenAPI not working
- Make sure your endpoints use `.WithOpenApi()` extension
- Check that the Microsoft.AspNetCore.OpenApi package is referenced

### AppHost configuration not working
- Ensure the service uses `AddServiceDefaults()` and `MapScalarDefaults()`
- Verify that the project reference in AppHost is correct
- Check that the `WithScalarApiDocumentation()` extension is applied to the correct resource