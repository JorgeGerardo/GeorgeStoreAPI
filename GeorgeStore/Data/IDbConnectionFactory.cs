using System.Data;

namespace GeorgeStore.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
