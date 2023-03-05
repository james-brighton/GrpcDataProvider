using JamesBrighton.Data;
using JamesBrighton.Data.GrpcClient;

await using var connection = GrpcClientFactory.Instance.CreateConnection() as IAsyncRemoteConnection;
if (connection == null) return;
var connectionStringBuilder = GrpcClientFactory.Instance.CreateConnectionStringBuilder();
connectionStringBuilder["GrpcServer"] = args[0];
connection.ConnectionString = connectionStringBuilder.ToString() ?? "";
connection.ServerProviderInvariantName = args[1];
connectionStringBuilder = GrpcClientFactory.Instance.CreateConnectionStringBuilder();
connectionStringBuilder[args[2].Split('=')[0]] = args[2].Split('=')[1];
connectionStringBuilder[args[3].Split('=')[0]] = args[3].Split('=')[1];
connectionStringBuilder[args[4].Split('=')[0]] = args[4].Split('=')[1];
connectionStringBuilder[args[5].Split('=')[0]] = args[5].Split('=')[1];
connection.ServerConnectionString = connectionStringBuilder.ToString() ?? "";

await connection.OpenAsync();
await using var transaction = await connection.BeginTransactionAsync();
await using var command = await connection.CreateCommandAsync();
command.Transaction = transaction;
command.CommandText = args[6];
if (args.Length >= 8)
{
    var parameter = command.CreateParameter();
    parameter.ParameterName = args[7];
    parameter.Value = int.Parse(args[8]);
    command.Parameters.Add(parameter);
}

try
{
    await using var reader = await command.ExecuteReaderAsync();
    var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3600));
    while (await reader.ReadAsync(cancellationTokenSource.Token))
    {
        var list = new List<string>();
        for (var i = 0; i < reader.FieldCount; i++)
        {
            list.Add(reader.GetName(i) + (!reader.IsDBNull(i) ? (": " + reader[i] + " (") : ": <Empty> (") + reader.GetDataTypeName(i) + ")");
        }
        Console.WriteLine(string.Join(", ", list));
    }

    await transaction.CommitAsync(cancellationTokenSource.Token);
}
catch (GrpcDataException e)
{
    await transaction.RollbackAsync();
    var list = new List<string>();
    var s = e.ClassName + ": " + e.Message;
    for (var i = 0; i < e.GetPropertyCount(); i++)
        list.Add(e.GetPropertyName(i) + ": " + e[i]);
    Console.WriteLine(s + Environment.NewLine + string.Join(Environment.NewLine, list));
}