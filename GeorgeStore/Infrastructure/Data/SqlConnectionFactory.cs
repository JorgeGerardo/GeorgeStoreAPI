using Npgsql;
using System.Data;

namespace GeorgeStore.Infrastructure.Data;

public class SqlConnectionFactory(IConfiguration _config) : IDbConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(
            _config.GetConnectionString("GeorgeStoreConnection")
        );
    }
}