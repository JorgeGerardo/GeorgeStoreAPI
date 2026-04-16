using Microsoft.Data.SqlClient;
using System.Data;

namespace GeorgeStore.Infrastructure.Data;

public class SqlConnectionFactory(IConfiguration _config) : IDbConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        return new SqlConnection(
            _config.GetConnectionString("GeorgeStoreConnection")
        );
    }
}