using System;
using MySqlConnector;

namespace BS.Utils;

public class Database : IDisposable
{
    private MySqlConnection? _connection;

    private static readonly MySqlConnectionStringBuilder _connectionStringBuilder =
        Settings.DatabaseConnectionStringBuilder;

    public Database()
    {
        Open();
    }

    public void SetData(string sql)
    {
        var command = new MySqlCommand(sql, _connection);
        command.ExecuteNonQuery();
    }

    public MySqlDataReader GetData(string sql)
    {
        var command = new MySqlCommand(sql, _connection);
        var reader = command.ExecuteReader();

        return reader;
    }

    public int GetValue(string sql)
    {
        var command = new MySqlCommand(sql, _connection);
        object result = command.ExecuteScalar();
        return (int) Convert.ToInt64(result);
    }

    private void Open()
    {
        _connection = new MySqlConnection(_connectionStringBuilder.ConnectionString);
        _connection.Open();
    }
    
    private void Close()
    {
        _connection?.Close();
    }
    
    public void Dispose()
    {
        Close();
    }
    
    ~Database()
    {
        Dispose();
    }
}