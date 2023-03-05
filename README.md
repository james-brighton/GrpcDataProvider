# Introduction

GrpcDataProvider is a .NET Data Provider for gRPC and provides data access to gRPC services. It uses the JamesBrighton.Data.GrpcClient namespace.

## Usage

JamesBrighton.Data.GrpcServer contains a basic ASP.NET application which should be the base of your gRPC server. To use the client, add a reference to JamesBrighton.Data.GrpcClient and start your database communication in the same way with any other ADO.NET client.

An example (with Firebird):

````csharp
await using var connection = GrpcClientFactory.Instance.CreateConnection() as IAsyncRemoteConnection;
connection.ConnectionString = "GrpServer=http://localhost:5056";
connection.ServerProviderInvariantName = "FirebirdSql.Data.FirebirdClient";
connection.ServerConnectionString = "UserId=SYSDBA;Password=masterkey;Database=localhost:/Library/Frameworks/Firebird.framework/Versions/A/Resources/examples/empbuild/employee.fdb;WireCrypt=Required";

await connection.OpenAsync();
await using var transaction = await connection.BeginTransactionAsync();
await using var command = await connection.CreateCommandAsync();
command.Transaction = transaction;
command.CommandText = "SELECT * FROM EMPLOYEE WHERE EMP_NO > @EMP_NO";
var parameter = command.CreateParameter();
parameter.ParameterName = "@EMP_NO";
parameter.Value = 0;
command.Parameters.Add(parameter);

try
{
    await using var reader = await command.ExecuteReaderAsync();
    var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3600));
    while (await reader.ReadAsync(cancellationTokenSource.Token))
    {
        // Use reader[int]/reader[string] here to access the data
    }

    await transaction.CommitAsync(cancellationTokenSource.Token);
}
catch (GrpcDataException e)
{
    await transaction.RollbackAsync();
    // Handle the exception
}
````
For more information, see JamesBrighton.Data.GrpcTest.AppClient, JamesBrighton.Data.GrpcTest.ConsoleClient or JamesBrighton.Data.GrpcTest.WebClient on how to use the project.

**Note:** The implementation is not yet complete. Custom interfaces are used to mask the fact that DbConnection, DbCommand, DbTransaction, and so on, are not fully realized.