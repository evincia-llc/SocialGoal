using Microsoft.Data.SqlClient;

namespace SocialGoal.Web.Tests.TestSupport;

/// <summary>
/// LocalDB helpers for the Sprint 5 spikes. Same conventions as the legacy
/// characterization harness: (localdb)\MSSQLLocalDB, dedicated throwaway
/// catalogs created fresh and dropped after the run, never real data.
/// </summary>
public static class LocalDb
{
    public const string ServerConnectionString =
        @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Encrypt=False";

    public static string ConnectionStringFor(string database) =>
        $@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog={database};Integrated Security=True;Encrypt=False";

    public static void CreateDatabase(string database)
    {
        using var connection = new SqlConnection(ServerConnectionString);
        connection.Open();
        Execute(connection, $"DROP DATABASE IF EXISTS [{database}]");
        Execute(connection, $"CREATE DATABASE [{database}]");
    }

    public static void DropDatabase(string database)
    {
        using var connection = new SqlConnection(ServerConnectionString);
        connection.Open();
        Execute(connection,
            $"IF DB_ID('{database}') IS NOT NULL ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE");
        Execute(connection, $"DROP DATABASE IF EXISTS [{database}]");
    }

    /// <summary>
    /// Runs a semicolon-terminated DDL script (the schema baseline has no
    /// semicolons inside statements, so a plain split is exact).
    /// </summary>
    public static void ExecuteScript(string database, string script)
    {
        using var connection = new SqlConnection(ConnectionStringFor(database));
        connection.Open();
        foreach (var statement in script.Split(';'))
        {
            if (!string.IsNullOrWhiteSpace(statement))
            {
                Execute(connection, statement);
            }
        }
    }

    public static void Execute(string database, string sql)
    {
        using var connection = new SqlConnection(ConnectionStringFor(database));
        connection.Open();
        Execute(connection, sql);
    }

    /// <summary>
    /// Walks up from the test assembly to the repo root and returns the path of
    /// the schema baseline of record.
    /// </summary>
    public static string SchemaBaselinePath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "docs", "schema", "schema-baseline.sql");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            "docs/schema/schema-baseline.sql not found above the test assembly; the spike tests must run from a repo checkout.");
    }

    private static void Execute(SqlConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
