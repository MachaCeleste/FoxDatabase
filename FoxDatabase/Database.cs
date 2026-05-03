using System.Diagnostics;
using Microsoft.Data.Sqlite;

namespace FoxDatabase;

public class Database
{
    private readonly string _connectionString;

    public Database(string dbFilePath)
    {
        _connectionString = $"Data Source={dbFilePath}";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA journal_mode=WAL;";
        command.ExecuteNonQuery();
    }

    public int ExecuteNonQuery(string query, Dictionary<string, object>? parameters = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        if (Debugger.IsAttached) ValidateQuery(query, connection);

        using var command = connection.CreateCommand();
        command.CommandText = query;

        if (parameters != null)
            foreach (var param in parameters)
                command.Parameters.AddWithValue(param.Key, param.Value);

        return command.ExecuteNonQuery();
    }

    public List<Dictionary<string, object>> ExecuteQuery(string query, Dictionary<string, object>? parameters = null)
    {
        var results = new List<Dictionary<string, object>>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        if (Debugger.IsAttached) ValidateQuery(query, connection);

        using var command = connection.CreateCommand();
        command.CommandText = query;

        if (parameters != null)
            foreach (var param in parameters)
                command.Parameters.AddWithValue(param.Key, param.Value);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row.Add(reader.GetName(i), reader.GetValue(i));
            }
            results.Add(row);
        }
        return results;
    }

    private void ValidateQuery(string query, SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"EXPLAIN {query}";
        try
        {
            command.ExecuteNonQuery();
        }
        catch (SqliteException ex)
        {
            Debugger.Break();
            throw;
        }
    }
}
