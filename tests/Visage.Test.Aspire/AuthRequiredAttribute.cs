using TUnit.Core;

namespace Visage.Test.Aspire;

/// <summary>
/// Skips the test when Auth0 test environment variables are not configured.
/// </summary>
internal sealed class AuthRequiredAttribute() : SkipAttribute("Auth0 test configuration is missing. Set AUTH0_* and TEST_USER_* env vars to run RequiresAuth tests.")
{
    public override Task<bool> ShouldSkip(TestRegisteredContext context)
        => Task.FromResult(!TestAppContext.IsAuthConfigured());
}
