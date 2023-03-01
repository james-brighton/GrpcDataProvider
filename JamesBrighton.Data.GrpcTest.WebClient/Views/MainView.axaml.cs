using Avalonia.Controls;
using Avalonia.Interactivity;
using JamesBrighton.Data.GrpcClient;

namespace JamesBrighton.Data.GrpcTest.WebClient.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    async void ButtonClick(object? sender, RoutedEventArgs e)
    {
        var textBlock = this.FindControl<TextBlock>("TextBlock");
        if (textBlock == null) return;
        await using var connection = GrpcClientFactory.Instance.CreateConnection() as IAsyncGrpcConnection;
        if (connection == null) return;
        connection.ConnectionString = "http://localhost:5056/";
        connection.ServerProviderInvariantName = "FirebirdSql.Data.FirebirdClient";
        var connectionStringBuilder = GrpcClientFactory.Instance.CreateConnectionStringBuilder();
        connectionStringBuilder["UserId"] = "SYSDBA";
        connectionStringBuilder["Password"] = "m4573rk3y";
        connectionStringBuilder["Database"] = "localhost:/Library/Frameworks/Firebird.framework/Versions/A/Resources/examples/empbuild/employee.fdb";
        connectionStringBuilder["WireCrypt"] = "Required";
        connection.ServerConnectionString = connectionStringBuilder.ToString() ?? "";
        try
        {
            await connection.OpenAsync();
        }
        catch (Grpc.Core.RpcException e1)
        {
            textBlock.Text = e1.Message + "\n" + e1.StackTrace;
            return;
        }
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
            var lines = new List<string>();
            while (await reader.ReadAsync(CancellationToken.None))
            {
                var line = new List<string>();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    line.Add(reader.GetName(i) + (!reader.IsDBNull(i) ? (": " + reader[i] + " (") : ": <Empty> (") + reader.GetDataTypeName(i) + ")");
                }
                lines.Add(string.Join(", ", line));
            }
            textBlock.Text = string.Join(Environment.NewLine, lines);

            await transaction.CommitAsync(CancellationToken.None);
        }
        catch (GrpcDataException e1)
        {
            await transaction.RollbackAsync();
            var list = new List<string>();
            var s = e1.ClassName + ": " + e1.Message;
            for (var i = 0; i < e1.GetPropertyCount(); i++)
                list.Add(e1.GetPropertyName(i) + ": " + e1[i]);
            textBlock.Text = s + Environment.NewLine + string.Join(Environment.NewLine, list);
        }
    }
}