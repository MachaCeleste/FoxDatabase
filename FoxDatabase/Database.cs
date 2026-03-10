using Microsoft.Data.Sqlite;

namespace FoxDatabase;

public class Database : IDisposable
{
    private readonly string _connectionString;
    private bool _disposed = false;

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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                SqliteConnection.ClearAllPools();
            }
            _disposed = true;
        }
    }
}
