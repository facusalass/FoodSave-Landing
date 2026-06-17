using Npgsql;

namespace Foodsave.Web.Data
{
    public static class DatabaseConnectionStringResolver
    {
        public static string Resolve(IConfiguration configuration)
        {
            var configuredConnection =
                configuration.GetConnectionString("DefaultConnection");

            if (!string.IsNullOrWhiteSpace(configuredConnection))
            {
                return configuredConnection;
            }

            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

            if (!string.IsNullOrWhiteSpace(databaseUrl))
            {
                return ConvertDatabaseUrl(databaseUrl);
            }

            throw new InvalidOperationException(
                "No se configuró PostgreSQL. Definí " +
                "ConnectionStrings__DefaultConnection o DATABASE_URL.");
        }

        public static string ConvertDatabaseUrl(string databaseUrl)
        {
            if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
            {
                throw new InvalidOperationException(
                    "DATABASE_URL debe usar el esquema postgres:// o postgresql://.");
            }

            var credentials = uri.UserInfo.Split(':', 2);
            if (credentials.Length != 2)
            {
                throw new InvalidOperationException(
                    "DATABASE_URL debe incluir usuario y contraseña.");
            }

            var database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
            if (string.IsNullOrWhiteSpace(uri.Host) ||
                string.IsNullOrWhiteSpace(database))
            {
                throw new InvalidOperationException(
                    "DATABASE_URL debe incluir host y nombre de base de datos.");
            }

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.IsDefaultPort ? 5432 : uri.Port,
                Username = Uri.UnescapeDataString(credentials[0]),
                Password = Uri.UnescapeDataString(credentials[1]),
                Database = database,
                Pooling = true
            };

            ApplyQueryParameters(builder, uri.Query);

            return builder.ConnectionString;
        }

        private static void ApplyQueryParameters(
            NpgsqlConnectionStringBuilder builder,
            string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return;
            }

            foreach (var item in query.TrimStart('?').Split(
                         '&',
                         StringSplitOptions.RemoveEmptyEntries))
            {
                var pair = item.Split('=', 2);
                var key = Uri.UnescapeDataString(pair[0]);
                var value = pair.Length == 2
                    ? Uri.UnescapeDataString(pair[1])
                    : string.Empty;

                if (key.Equals("sslmode", StringComparison.OrdinalIgnoreCase))
                {
                    builder.SslMode = ParseSslMode(value);
                }
            }
        }

        private static SslMode ParseSslMode(string value)
        {
            return value.ToLowerInvariant() switch
            {
                "disable" => SslMode.Disable,
                "prefer" => SslMode.Prefer,
                "require" => SslMode.Require,
                "verify-ca" => SslMode.VerifyCA,
                "verify-full" => SslMode.VerifyFull,
                _ => throw new InvalidOperationException(
                    $"El sslmode '{value}' de DATABASE_URL no es válido.")
            };
        }
    }
}
