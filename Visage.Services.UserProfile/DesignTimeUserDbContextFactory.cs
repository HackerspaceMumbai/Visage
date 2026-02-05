using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Visage.Services.UserProfile;

internal sealed class DesignTimeUserDbContextFactory : IDesignTimeDbContextFactory<UserDB>
{
    public UserDB CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UserDB>();

        // For design-time operations (dotnet ef ...), Aspire doesn't inject connection strings.
        // Provide one via env var (preferred) or fallback to LocalDB for dev convenience.
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__userprofiledb")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__UserDB")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=userprofiledb;Trusted_Connection=True;TrustServerCertificate=True";

        optionsBuilder.UseSqlServer(connectionString);

        return new UserDB(optionsBuilder.Options);
    }
}
