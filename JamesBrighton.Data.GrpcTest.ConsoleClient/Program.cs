using JamesBrighton.Data;
using JamesBrighton.Data.GrpcClient;

await using var connection = GrpcClientFactory.Instance.CreateConnection() as IAsyncGrpcConnection;
if (connection == null) return;
connection.ConnectionString = args[0];
connection.ServerProviderInvariantName = args[1];
connection.ServerConnectionString = args[2];

await connection.OpenAsync();
await using var transaction = await connection.BeginTransactionAsync();
await using var command = await connection.CreateCommandAsync();
command.Transaction = transaction;
command.CommandText = args[3];
if (args.Length >= 5)
{
    var parameter = command.CreateParameter();
    parameter.ParameterName = args[4];
    parameter.Value = int.Parse(args[5]);
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