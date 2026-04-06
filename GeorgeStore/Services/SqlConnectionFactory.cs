using GeorgeStore.Data;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GeorgeStore.Services;

public class SqlConnectionFactory(IConfiguration _config) : IDbConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        return new SqlConnection(
            _config.GetConnectionString("GeorgeStoreConnection")
        );
    }
}