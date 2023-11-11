using System.Data;
using System.Globalization;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using JamesBrighton.Data.GrpcClient.Common;
using JamesBrighton.Data.GrpcClient.Grpc;

namespace JamesBrighton.Data.GrpcTest.SharedClient.Views;

public partial class MainView : UserControl
{
	public MainView()
	{
		InitializeComponent();
		DataGrid.ItemsSource = items;
	}

	async void ButtonClick(object? sender, RoutedEventArgs e)
	{
		items.Clear();
		DataGrid.Columns.Clear();
		OutputTextBlock.Text = "";

		await using var connection = GrpcClientFactory.Instance.CreateConnection() as IAsyncRemoteConnection;
		if (connection == null) return;
		SetupConnection(connection);
		try
		{
			await connection.OpenAsync();
		}
		catch (Grpc.Core.RpcException e1)
		{
			OutputTextBlock.Text = e1.Message + "\n" + e1.StackTrace;
			return;
		}
		catch (DataException e1)
		{
			OutputTextBlock.Text = e1.Message + "\n" + e1.StackTrace;
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
		catch (RemoteDataException e1)
		{
			await ShowException(transaction, e1);
		}
	}

	void SetupConnection(IAsyncRemoteConnection connection)
	{
		var connectionStringBuilder = GrpcClientFactory.Instance.CreateConnectionStringBuilder();
		connectionStringBuilder["GrpcServer"] = "http://localhost:5056/";
		connection.ConnectionString = connectionStringBuilder.ToString() ?? "";
		connection.ServerProviderInvariantName = "FirebirdSql.Data.FirebirdClient";
		connectionStringBuilder = GrpcClientFactory.Instance.CreateConnectionStringBuilder();
		connectionStringBuilder["UserId"] = UserIdTextBox.Text ?? "";
		connectionStringBuilder["Password"] = PasswordTextBox.Text ?? "";
		connectionStringBuilder["Database"] = DatabaseTextBox.Text ?? "";
		connectionStringBuilder["WireCrypt"] = "Required";
		connection.ServerConnectionString = connectionStringBuilder.ToString() ?? "";
	}

	void SetupCommand(IDbTransaction transaction, IDbCommand command)
	{
		command.Transaction = transaction;
		var query = QueryTextBox.Text ?? "";
		command.CommandText = query;
		var queryElements = query.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
		var index = queryElements.FindIndex(x => x.StartsWith('@'));
		if (index < 0)
			return;
		var value = GetParameterValue();
		if (value == null)
			return;
		var parameter = command.CreateParameter();
		parameter.ParameterName = queryElements[index];
		parameter.Value = value;
		command.Parameters.Add(parameter);
	}

	object? GetParameterValue()
	{
		var value = ParameterTextBox.Text ?? "";
		if (value.StartsWith('\"') && value.EndsWith('\"'))
			return value[1..^1];
		if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
			return d;
		if (int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var i))
			return i;

		return null;
	}

	void SetupColumns(IDataRecord reader)
	{
		for (var i = 0; i < reader.FieldCount; i++)
		{
			var dataGridTextColumn = new DataGridTextColumn
			{
				Binding = new Binding("[" + i + "]"),
				Header = new TextBlock { Text = reader.GetName(i) + Environment.NewLine + reader.GetDataTypeName(i) },
				Width = DataGridLength.Auto
			};
			DataGrid.Columns.Add(dataGridTextColumn);
		}
	}

	void ReadLine(IDataRecord reader)
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

	async Task ShowException(IAsyncDbTransaction transaction, RemoteDataException exception)
	{
		await transaction.RollbackAsync();
		var list = new List<string>();
		var s = exception.ClassName + ": " + exception.Message;
		for (var i = 0; i < exception.GetPropertyCount(); i++)
			list.Add(exception.GetPropertyName(i) + ": " + exception[i]);
		OutputTextBlock.Text = s + Environment.NewLine + string.Join(Environment.NewLine, list);
	}

	readonly AvaloniaList<List<string>> items = new();
}