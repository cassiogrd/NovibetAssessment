using Microsoft.Data.SqlClient;

public class AdoConnectionService
{
    private readonly string _connectionString;

    public AdoConnectionService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
