# Quick Start: Running E2E Tests with Token-Based Auth

## ⚡ Quick Setup (5 minutes)

### 1️⃣ Configure Auth0 (One-Time Setup)

#### A. Create Test User
1. **Auth0 Dashboard** → User Management → Users → **Create User**
2. Email: `test.playwright@hackmum.in`
3. Password: Strong password (save it!)
4. Connection: Username-Password-Authentication

#### B. Enable Password Grant
1. **Applications** → [Your Visage App] → **Advanced Settings** → **Grant Types**
2. ✅ Enable **Password**
3. **Save Changes**

#### C. Create Security Action (Whitelist Test Users)
1. **Actions** → **Flows** → **Login** → **+ Custom Action**
2. Name: `Restrict Password Grant to Test Users`
3. Copy code from `tests/Visage.Test.Aspire/README.md` (Auth0 Action section)
4. **Deploy** → Drag into Login flow → **Apply**

### 2️⃣ Set Environment Variables

**Option A: Interactive Script (Recommended)**
```powershell
# Run the setup script - it will prompt for all values
pwsh tests/Visage.Test.Aspire/setup-test-env.ps1
```

**Option B: Manual Setup**
```powershell
# Auth0 Configuration (from Auth0 Dashboard)
$env:AUTH0_DOMAIN = "your-tenant.auth0.com"
$env:AUTH0_CLIENT_ID = "your-client-id"
$env:AUTH0_CLIENT_SECRET = "your-client-secret"
$env:AUTH0_AUDIENCE = "your-api-audience"

# Test User Credentials (whitelisted test account)
$env:TEST_USER_EMAIL = "test.playwright@hackmum.in"
$env:TEST_USER_PASSWORD = "your-test-password"

# Application URL
$env:TEST_BASE_URL = "https://localhost:5001"
```

### 3️⃣ Start Aspire App

```powershell
# In a separate terminal
dotnet run --project Visage.AppHost/Visage.AppHost.csproj
```

Wait until you see:
```
✅ Now listening on: https://localhost:5001
```

### 4️⃣ Run E2E Tests

```powershell
# Run all E2E tests
dotnet test --filter "Category=E2E"

# Run only draft persistence tests
dotnet test --filter "Category=DraftPersistence"

# Run smoke tests only (quick sanity check)
dotnet test --filter "Category=Smoke"
```

---

## 🔍 Troubleshooting

### "AUTH0_DOMAIN not set" Error
- **Fix**: Run `setup-test-env.ps1` or manually set environment variables
- **Verify**: `echo $env:AUTH0_DOMAIN` should show your tenant

### "Email is not authorized" Error
- **Fix**: Add email to `Auth0TestHelper.cs` whitelist AND Auth0 Action
- **Location**: `tests/Visage.Test.Aspire/Auth0TestHelper.cs` line 17

### "401 Unauthorized" During Test
- **Fix 1**: Verify Password Grant is enabled in Auth0
- **Fix 2**: Check Auth0 Action is deployed and in Login flow
- **Fix 3**: Verify test user exists and credentials are correct

### Test Timeout / Page Not Loading
- **Fix**: Ensure Aspire app is running (`dotnet run --project Visage.AppHost`)
- **Verify**: Navigate to `https://localhost:5001` in browser

### Selector Not Found Error
- **Fix**: Update selectors in test to match actual UI
- **Debug**: Add screenshot: `await Page.ScreenshotAsync(new() { Path = "debug.png" })`

---

## 📚 Additional Resources

- **Full Security Documentation**: `tests/Visage.Test.Aspire/README.md`
- **Test Implementation**: `tests/Visage.Test.Aspire/ProfileDraftPersistenceTests.cs`
- **Auth Helper**: `tests/Visage.Test.Aspire/Auth0TestHelper.cs`

---

## 🎯 Success Criteria

When tests pass, you should see:
```
✅ Passed! - WhenUserFillsProfileThenDraftSavesAndRestores
✅ Passed! - WhenUserDeletesDraftThenFieldsAreCleared

Test Run Successful.
Total tests: 2, Passed: 2
```

---

## 🔐 Security Reminder

- ✅ Test emails are whitelisted in code and Auth0 Action
- ✅ Environment check prevents Production usage
- ✅ Never commit credentials to git
- ✅ Password Grant only enabled for test application
