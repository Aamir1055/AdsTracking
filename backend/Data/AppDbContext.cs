using MySqlConnector;

namespace AdsTracking.Api.Data;

/// <summary>
/// Simple MySQL connection factory using MySqlConnector + Dapper.
/// No EF Core — no model building overhead.
/// </summary>
public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public MySqlConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }
}
