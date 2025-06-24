using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add services with Scalar API documentation
var sampleAPI = builder.AddProject<Projects.SampleAPI>("sample-api")
    .WithScalarApiDocumentation("Sample API");

var userAPI = builder.AddProject<Projects.UserAPI>("user-api")
    .WithScalarApiDocumentation("User Management API", "/docs");

var orderAPI = builder.AddProject<Projects.OrderAPI>("order-api")
    .WithScalarApiDocumentation("Order Processing API");

// Add frontend application
var webapp = builder.AddProject<Projects.WebApp>("webapp")
    .WithReference(sampleAPI)
    .WithReference(userAPI)
    .WithReference(orderAPI);

builder.Build().Run();