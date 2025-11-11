# Visage E2E Tests

## Security: Password Grant Restrictions

This test suite uses Auth0 Resource Owner Password Grant for automated E2E testing. To prevent security vulnerabilities:

### ✅ Security Measures in Place

1. **Email Whitelist** - `Auth0TestHelper.cs` only allows specific test emails:
   - `test.playwright@hackmum.in`
   - `e2e.test@hackmum.in`
   - `ci.test@hackmum.in`

2. **Environment Check** - Password Grant is disabled in `Production` environments

3. **Auth0 Action** - Server-side whitelist in Auth0 Login flow (see Auth0 Dashboard → Actions)

4. **Separate Test Application** - Uses dedicated Auth0 app credentials (not production)

### 🔒 Best Practices

- **Never** commit test credentials to source control
- **Never** use production user emails in tests
- **Never** enable Password Grant on production Auth0 applications
- **Always** use dedicated test users with minimal privileges
- **Always** rotate test credentials regularly (every 90 days)

### 🚨 If Password Grant Must Be Disabled

Switch to UI-based authentication in tests:
1. Remove token-based auth calls
2. Use Playwright to automate Auth0 login UI
3. Trade-off: Slower tests, more flaky, but no Password Grant required

### 📋 Adding a New Test User

1. **Auth0 Dashboard** → Users → Create User
2. Email: `{purpose}.test@hackmum.in` (e.g., `ci.test@hackmum.in`)
3. Add email to `AllowedTestEmails` in `Auth0TestHelper.cs`
4. Add email to Auth0 Action whitelist
5. Set `TEST_USER_EMAIL` and `TEST_USER_PASSWORD` environment variables

### 🔍 Verifying Security

Run this command to verify email whitelist enforcement:

```powershell
# Should throw SecurityException
$env:TEST_USER_EMAIL = "unauthorized@example.com"
dotnet test --filter "FullyQualifiedName~Auth0TestHelper"
```

### 📚 References

- [Auth0 Resource Owner Password Grant](https://auth0.com/docs/get-started/authentication-and-authorization-flow/resource-owner-password-flow)
- [Auth0 Actions](https://auth0.com/docs/customize/actions)
- [OWASP Password Grant Risks](https://owasp.org/www-community/attacks/Password_Spraying_Attack)
