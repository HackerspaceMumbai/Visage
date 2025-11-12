using System.Net;
using Visage.Shared.Models;

namespace Visage.FrontEnd.Shared.Services;

public sealed record RegistrationResult(
    bool IsSuccess,
    Registrant? Registrant,
    HttpStatusCode StatusCode,
    string? ErrorMessage)
{
    public static RegistrationResult Success(Registrant registrant, HttpStatusCode statusCode) =>
        new(true, registrant, statusCode, null);

    public static RegistrationResult Failure(HttpStatusCode statusCode, string? errorMessage) =>
        new(false, null, statusCode, errorMessage);
}
