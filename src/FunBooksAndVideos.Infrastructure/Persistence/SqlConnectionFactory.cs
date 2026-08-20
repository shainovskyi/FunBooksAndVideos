using System.Data;
using Microsoft.Data.SqlClient;

namespace FunBooksAndVideos.Infrastructure.Persistence;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class SqlConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public IDbConnection CreateConnection() => new SqlConnection(connectionString);
}
