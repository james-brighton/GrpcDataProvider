using System.Data;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using JamesBrighton.Data.GrpcClient;

namespace JamesBrighton.Data.GrpcTest.SharedClient.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        dataGrid.Items = items;
    }

    async void ButtonClick(object? sender, RoutedEventArgs e)
    {
        items.Clear();
        dataGrid.Columns.Clear();
        outputTextBlock.Text = "";

        await using var connection = GrpcClientFactory.Instance.CreateConnection() as IAsyncGrpcConnection;
        if (connection == null) return;
        SetupConnection(connection);
        try
        {
            await connection.OpenAsync();
        }
        catch (Grpc.Core.RpcException e1)
        {
            outputTextBlock.Text = e1.Message + "\n" + e1.StackTrace;
            return;
        }
        catch (GrpcDataException e1)
        {
            outputTextBlock.Text = e1.Message + "\n" + e1.StackTrace;
            return;
        }
        await using var transaction = await connection.BeginTransactionAsync();
        await using var command = await connection.CreateCommandAsync();
        SetupCommand(transaction, command);

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            var firstTime = true;
            while (await reader.ReadAsync(CancellationToken.None))
            {
                if (firstTime)
                {
                    SetupColumns(reader);
                    firstTime = false;
                }
                ReadLine(reader);
            }

            await transaction.CommitAsync(CancellationToken.None);
        }
        catch (GrpcDataException e1)
        {
            await ShowException(transaction, e1);
        }
    }

    void SetupConnection(IAsyncGrpcConnection connection)
    {
        var connectionStringBuilder = GrpcClientFactory.Instance.CreateConnectionStringBuilder();
        connectionStringBuilder["GrpcServer"] = "http://localhost:5056/";
        connection.ConnectionString = connectionStringBuilder.ToString() ?? "";
        connection.ServerProviderInvariantName = "FirebirdSql.Data.FirebirdClient";
        connectionStringBuilder = GrpcClientFactory.Instance.CreateConnectionStringBuilder();
        connectionStringBuilder["UserId"] = userIdTextBox.Text ?? "";
        connectionStringBuilder["Password"] = passwordTextBox.Text ?? "";
        connectionStringBuilder["Database"] = databaseTextBox.Text ?? "";
        connectionStringBuilder["WireCrypt"] = "Required";
        connection.ServerConnectionString = connectionStringBuilder.ToString() ?? "";
    }

    static void SetupCommand(IDbTransaction transaction, IDbCommand command)
    {
        command.Transaction = transaction;
        command.CommandText = "SELECT * FROM EMPLOYEE WHERE EMP_NO > @EMP_NO";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@EMP_NO";
        parameter.Value = 0;
        command.Parameters.Add(parameter);
    }

    void SetupColumns(IDataRecord reader)
    {
        for (var i = 0; i < reader.FieldCount; i++)
        {
            var dataGridTextColumn = new DataGridTextColumn
            {
                Binding = new Binding("[" + i + "]"),
                Header = new TextBlock { Text = reader.GetName(i) + " (" + reader.GetDataTypeName(i) + ")" },
                Width = DataGridLength.Auto
            };
            dataGrid.Columns.Add(dataGridTextColumn);
        }
    }

    void ReadLine(IAsyncDataReader reader)
    {
        var item = new List<string>();
        for (var i = 0; i < reader.FieldCount; i++)
        {
            if (!reader.IsDBNull(i))
                item.Add(reader[i].ToString() ?? "");
            else
                item.Add("<Empty>");
        }
        items.Add(item);
    }

    async Task ShowException(IAsyncDbTransaction transaction, GrpcDataException exception)
    {
        await transaction.RollbackAsync();
        var list = new List<string>();
        var s = exception.ClassName + ": " + exception.Message;
        for (var i = 0; i < exception.GetPropertyCount(); i++)
            list.Add(exception.GetPropertyName(i) + ": " + exception[i]);
        outputTextBlock.Text = s + Environment.NewLine + string.Join(Environment.NewLine, list);
    }

    readonly AvaloniaList<List<string>> items = new();
}