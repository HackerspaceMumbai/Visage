using TUnit.Core;

namespace Visage.Test.Aspire;

internal static class AuthTestGuard
{
    public static void RequireAuthConfigured()
    {
        if (TestAppContext.IsAuthConfigured())
        {
            return;
        }

        Skip.Test("Auth0 test configuration is missing. Set AUTH0_* and TEST_USER_* env vars to run RequiresAuth tests.");
    }
}
