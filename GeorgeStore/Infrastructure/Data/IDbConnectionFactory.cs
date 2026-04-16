using System.Data;

namespace GeorgeStore.Infrastructure.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
