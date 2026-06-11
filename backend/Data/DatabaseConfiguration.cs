using Microsoft.EntityFrameworkCore;

namespace SetlistSocial.Api.Data;

public static class DatabaseConfiguration
{
    private const string SqliteProvider = "sqlite";
    private const string PostgreSqlProvider = "postgresql";

    public static void Configure(
        DbContextOptionsBuilder options,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        Configure(options, configuration, environment.EnvironmentName);
    }

    public static void Configure(
        DbContextOptionsBuilder options,
        IConfiguration configuration,
        string? environmentName)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        var provider = ResolveProvider(configuration, connectionString, environmentName);

        if (provider == SqliteProvider)
        {
            options.UseSqlite(connectionString);
            return;
        }

        options.UseNpgsql(connectionString);
    }

    private static string ResolveProvider(
        IConfiguration configuration,
        string connectionString,
        string? environmentName)
    {
        var configuredProvider = configuration["Database:Provider"];
        var normalizedProvider = NormalizeProvider(configuredProvider);
        var isProduction = string.Equals(environmentName, "Production", StringComparison.OrdinalIgnoreCase);

        if (normalizedProvider == SqliteProvider && isProduction)
        {
            throw new InvalidOperationException("Production must use PostgreSQL. Configure ConnectionStrings__DefaultConnection for PostgreSQL.");
        }

        if (normalizedProvider is not null)
        {
            return normalizedProvider;
        }

        if (isProduction || LooksLikePostgreSql(connectionString))
        {
            return PostgreSqlProvider;
        }

        return SqliteProvider;
    }

    private static string? NormalizeProvider(string? provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return null;
        }

        return provider.Trim().ToLowerInvariant() switch
        {
            "sqlite" => SqliteProvider,
            "postgres" or "postgresql" or "npgsql" => PostgreSqlProvider,
            var invalid => throw new InvalidOperationException($"Unsupported database provider '{invalid}'. Use SQLite or PostgreSQL.")
        };
    }

    private static bool LooksLikePostgreSql(string connectionString)
    {
        return connectionString.StartsWith("Host=", StringComparison.OrdinalIgnoreCase)
            || connectionString.StartsWith("Server=", StringComparison.OrdinalIgnoreCase)
            || connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            || connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
            || connectionString.Contains("Username=", StringComparison.OrdinalIgnoreCase);
    }
}
