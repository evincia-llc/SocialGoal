using System.Globalization;
using Microsoft.Data.SqlClient;

namespace SocialGoal.Web.Tests.TestSupport;

/// <summary>
/// Reads the parts of a database's catalog the schema-parity tests compare:
/// table names, column facets, primary-key column sets, and foreign keys.
/// PK/index *names* are excluded by design -- the EF6 baseline uses
/// system-generated PK names.
/// </summary>
public static class SqlCatalog
{
    /// <summary>
    /// Every user table in dbo -- unfiltered, so a table appearing on one side
    /// only is a failure rather than something a filter can hide.
    /// </summary>
    public static SortedSet<string> ReadTableNames(string database)
    {
        var result = new SortedSet<string>(StringComparer.Ordinal);
        using var connection = new SqlConnection(LocalDb.ConnectionStringFor(database));
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT t.name FROM sys.tables t WHERE SCHEMA_NAME(t.schema_id) = 'dbo'";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(reader.GetString(0));
        }

        return result;
    }

    public static SortedDictionary<string, string> ReadColumns(string database, IReadOnlyCollection<string> tables)
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        using var connection = new SqlConnection(LocalDb.ConnectionStringFor(database));
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT t.name, c.name, ty.name, c.max_length, c.precision, c.scale, c.is_nullable, c.is_identity
            FROM sys.columns c
            JOIN sys.tables t ON t.object_id = c.object_id
            JOIN sys.types ty ON ty.user_type_id = c.user_type_id
            WHERE SCHEMA_NAME(t.schema_id) = 'dbo'
            """;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var table = reader.GetString(0);
            if (!tables.Contains(table))
            {
                continue;
            }

            var type = reader.GetString(2);
            var maxLength = reader.GetInt16(3);
            var facet = type switch
            {
                "nvarchar" => maxLength == -1 ? "nvarchar(max)" : $"nvarchar({maxLength / 2})",
                "varchar" => maxLength == -1 ? "varchar(max)" : $"varchar({maxLength})",
                "decimal" or "numeric" =>
                    $"{type}({reader.GetByte(4)},{reader.GetByte(5)})",
                _ => type,
            };
            var descriptor = string.Create(
                CultureInfo.InvariantCulture,
                $"{facet} {(reader.GetBoolean(6) ? "NULL" : "NOT NULL")}{(reader.GetBoolean(7) ? " identity" : "")}");
            result[$"{table}.{reader.GetString(1)}"] = descriptor;
        }

        return result;
    }

    public static SortedDictionary<string, string> ReadPrimaryKeys(string database, IReadOnlyCollection<string> tables)
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        using var connection = new SqlConnection(LocalDb.ConnectionStringFor(database));
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT t.name, c.name, ic.key_ordinal
            FROM sys.indexes i
            JOIN sys.tables t ON t.object_id = i.object_id
            JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
            JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
            WHERE i.is_primary_key = 1 AND SCHEMA_NAME(t.schema_id) = 'dbo'
            ORDER BY t.name, ic.key_ordinal
            """;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var table = reader.GetString(0);
            if (!tables.Contains(table))
            {
                continue;
            }

            var column = reader.GetString(1);
            result[table] = result.TryGetValue(table, out var existing) ? $"{existing}, {column}" : column;
        }

        return result;
    }

    /// <summary>
    /// Foreign keys whose parent AND referenced table are both in the given
    /// set (the baseline database also carries FKs from non-spike tables into
    /// spike tables; those are out of scope until Sprints 6-7).
    /// </summary>
    public static SortedSet<string> ReadForeignKeys(string database, IReadOnlyCollection<string> tables)
    {
        var result = new SortedSet<string>(StringComparer.Ordinal);
        using var connection = new SqlConnection(LocalDb.ConnectionStringFor(database));
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT fk.name, tp.name, cp.name, tr.name, cr.name, fk.delete_referential_action_desc
            FROM sys.foreign_keys fk
            JOIN sys.tables tp ON tp.object_id = fk.parent_object_id
            JOIN sys.tables tr ON tr.object_id = fk.referenced_object_id
            JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
            JOIN sys.columns cp ON cp.object_id = fkc.parent_object_id AND cp.column_id = fkc.parent_column_id
            JOIN sys.columns cr ON cr.object_id = fkc.referenced_object_id AND cr.column_id = fkc.referenced_column_id
            """;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var parent = reader.GetString(1);
            var referenced = reader.GetString(3);
            if (!tables.Contains(parent) || !tables.Contains(referenced))
            {
                continue;
            }

            result.Add(
                $"{reader.GetString(0)}: {parent}.{reader.GetString(2)} -> {referenced}.{reader.GetString(4)} ON DELETE {reader.GetString(5)}");
        }

        return result;
    }

    public static SortedSet<string> ReadNonPrimaryKeyIndexes(string database, IReadOnlyCollection<string> tables)
    {
        var result = new SortedSet<string>(StringComparer.Ordinal);
        using var connection = new SqlConnection(LocalDb.ConnectionStringFor(database));
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT t.name, i.name
            FROM sys.indexes i
            JOIN sys.tables t ON t.object_id = i.object_id
            WHERE i.is_primary_key = 0 AND i.type > 0 AND SCHEMA_NAME(t.schema_id) = 'dbo'
            """;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var table = reader.GetString(0);
            if (tables.Contains(table))
            {
                result.Add($"{table}: {reader.GetString(1)}");
            }
        }

        return result;
    }
}
