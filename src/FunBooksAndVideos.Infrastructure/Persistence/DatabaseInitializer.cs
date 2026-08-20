using System.Reflection;
using Dapper;
using Microsoft.Data.SqlClient;

namespace FunBooksAndVideos.Infrastructure.Persistence;

public class DatabaseInitializer(string connectionString)
{
    public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var masterConnectionString = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master"
        }.ConnectionString;

        var batches = ReadInitializationScript().Split(["\r\nGO", "\nGO"], StringSplitOptions.RemoveEmptyEntries);

        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync(cancellationToken);

        foreach (var batch in batches)
        {
            await connection.ExecuteAsync(new CommandDefinition(batch, cancellationToken: cancellationToken));
        }
    }

    private static string ReadInitializationScript()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .Single(x => x.EndsWith("Schema.sql", StringComparison.OrdinalIgnoreCase));

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
