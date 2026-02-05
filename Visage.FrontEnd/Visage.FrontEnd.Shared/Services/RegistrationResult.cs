using System.Net;
using Visage.Shared.Models;

namespace Visage.FrontEnd.Shared.Services;

/// <summary>
/// Result of a registration or user profile operation.
/// Generic to support both User and EventRegistration results.
/// </summary>
public sealed record RegistrationResult(
    bool IsSuccess,
    object? Entity,
    HttpStatusCode StatusCode,
    string? ErrorMessage)
{
    /// <summary>
    /// Gets the result entity as a User (for user profile operations).
    /// </summary>
    public User? User => Entity as User;

    /// <summary>
    /// Gets the result entity as an EventRegistration (for event registration operations).
    /// </summary>
    public EventRegistration? EventRegistration => Entity as EventRegistration;

    public static RegistrationResult Success(User user, HttpStatusCode statusCode) =>
        new(true, user, statusCode, null);

    public static RegistrationResult Success(EventRegistration registration, HttpStatusCode statusCode) =>
        new(true, registration, statusCode, null);

    public static RegistrationResult Failure(HttpStatusCode statusCode, string? errorMessage) =>
        new(false, null, statusCode, errorMessage);
}
