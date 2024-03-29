using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using JamesBrighton.DataProvider.Grpc;
using JamesBrighton.Data.Common;
using JamesBrighton.Data.Common.Helpers;
using JamesBrighton.Data.GrpcServer.Helpers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

using static JamesBrighton.DataProvider.Grpc.DatabaseService;
using DataException = JamesBrighton.DataProvider.Grpc.DataException;

namespace JamesBrighton.Data.GrpcServer.Services;

/// <summary>
/// Provides a gRPC service for working with databases.
/// </summary>
public class DatabaseService : DatabaseServiceBase
{
	/// <summary>
	/// List of clients.
	/// </summary>
	static readonly ConcurrentDictionary<string, DatabaseConnection> clients = new();
	/// <summary>
	/// The logger.
	/// </summary>
	readonly ILogger<DatabaseService> logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="DatabaseService"/> class.
	/// </summary>
	/// <param name="logger">The logger.</param>
	public DatabaseService(ILogger<DatabaseService> logger)
	{
		this.logger = logger;
	}

	/// <summary>
	/// Opens a connection to a database.
	/// </summary>
	/// <param name="request">The request parameters.</param>
	/// <param name="context">The server call context.</param>
	/// <returns>The connection identifier or an exception if an error occurred.</returns>
	public override async Task<OpenConnectionResponse> OpenConnection(OpenConnectionRequest request, ServerCallContext context)
	{
		var connectionIdentifier = GenerateIdentifier();
		var connection = CreateDbConnection(request.ProviderInvariantName, request.ConnectionString);
		if (connection == null)
		{
			var e = new InvalidOperationException($"Unable to get the connection for the provider {request.ProviderInvariantName}.");
			return new OpenConnectionResponse { ClientIdentifier = context.Peer, DataException = CreateException(e) };
		}
		try
		{
			await connection.OpenAsync();
		}
		catch (Exception e)
		{
			return new OpenConnectionResponse { ClientIdentifier = context.Peer, DataException = CreateException(e) };
		}

		clients[connectionIdentifier] = new DatabaseConnection(connection);
		return new OpenConnectionResponse { ClientIdentifier = context.Peer, ConnectionIdentifier = connectionIdentifier };
	}

	/// <summary>
	/// Closes a connection to a database.
	/// </summary>
	/// <param name="request">The request parameters.</param>
	/// <param name="context">The server call context.</param>
	/// <returns>An empty response.</returns>
	public override async Task<Empty> CloseConnection(CloseConnectionRequest request, ServerCallContext context)
	{
		if (clients.TryRemove(request.ConnectionIdentifier, out var connection))
			await connection.Connection.CloseAsync();
		return new Empty();
	}

	/// <summary>
	/// Creates a new command for a database connection.
	/// </summary>
	/// <param name="request">The request parameters.</param>
	/// <param name="context">The server call context.</param>
	/// <returns>The command identifier.</returns>
	public override async Task<CreateCommandResponse> CreateCommand(CreateCommandRequest request, ServerCallContext context)
	{
		await Task.Delay(0, context.CancellationToken);
		if (!clients.TryGetValue(request.ConnectionIdentifier, out var connection))
			return new CreateCommandResponse();

		var command = connection.Connection.CreateCommand();
		var commandIdentifier = GenerateIdentifier();
		connection.AddCommand(commandIdentifier, command);

		return new CreateCommandResponse { CommandIdentifier = commandIdentifier };
	}

	/// <summary>
	/// Destroys a command for a database connection.
	/// </summary>
	/// <param name="request">The request parameters.</param>
	/// <param name="context">The server call context.</param>
	/// <returns>An empty response.</returns>
	public override async Task<Empty> DestroyCommand(DestroyCommandRequest request, ServerCallContext context)
	{
		if (!clients.TryGetValue(request.ConnectionIdentifier, out var connection))
			return new Empty();

		await connection.DestroyCommandAsync(request.CommandIdentifier);
		return new Empty();
	}

	/// <summary>
	/// Begins a transaction on a database connection.
	/// </summary>
	/// <param name="request">The request parameters.</param>
	/// <param name="context">The server call context.</param>
	/// <returns>The transaction identifier.</returns>
	public override async Task<BeginTransactionResponse> BeginTransaction(BeginTransactionRequest request, ServerCallContext context)
	{
		if (!clients.TryGetValue(request.ConnectionIdentifier, out var connection))
			return new BeginTransactionResponse();

		var transaction = await connection.Connection.BeginTransactionAsync(IsolationLevelConverter.Convert(request.IsolationLevel));

		// If there's a client identifier, insert into the (temporary) table REMOTE_TRANSACTIONS
		if (!string.IsNullOrEmpty(request.ClientIdentifier) && connection.HasRemoteTransactionTable)
		{
			await using var command = connection.Connection.CreateCommand();
			command.Transaction = transaction;
			command.CommandText = "INSERT INTO REMOTE_TRANSACTIONS (REMOTE_ID) VALUES (@REMOTE_ID)";
			var parameter = command.CreateParameter();
			parameter.ParameterName = "@REMOTE_ID";
			parameter.Value = request.ClientIdentifier;
			command.Parameters.Add(parameter);
			try
			{
				await command.ExecuteNonQueryAsync();
				connection.HasRemoteTransactionTable = true;
			}
			catch (Exception)
			{
				// Swallow: table doesn't exist
				connection.HasRemoteTransactionTable = false;
			}
		}

		var transactionIdentifier = GenerateIdentifier();
		connection.AddTransaction(transactionIdentifier, transaction);

		return new BeginTransactionResponse { TransactionIdentifier = transactionIdentifier };
	}

	/// <summary>
	/// Given a provider name and connection string, create the DbProviderFactory and DbConnection.
	/// </summary>
	/// <param name="providerInvariantName">Invariant provider name.</param>
	/// <param name="connectionString">Connection string to use.</param>
	/// <returns>The DbConnection or null otherwise.</returns>
	static DbConnection? CreateDbConnection(string providerInvariantName, string connectionString)
	{
		DbProviderFactory factory;
		try
		{
			factory = DbProviderFactories.GetFactory(providerInvariantName);
		}
		catch (ArgumentException)
		{
			// Cannot find factory
			return null;
		}

		var connection = factory.CreateConnection();
		if (connection == null)
			return null;
		connection.ConnectionString = connectionString;
		return connection;
	}

	/// <summary>
	/// Commits a transaction on a database connection.
	/// </summary>
	/// <param name="request">The request parameters.</param>
	/// <param name="context">The server call context.</param>
	/// <returns>An empty response or an exception if an error occurred.</returns>
	public override async Task<CommitTransactionResponse> CommitTransaction(CommitTransactionRequest request, ServerCallContext context)
	{
		if (!clients.TryGetValue(request.ConnectionIdentifier, out var connection))
			return new CommitTransactionResponse();

		try
		{
			await connection.CommitAndDestroyAsync(request.TransactionIdentifier);
			return new CommitTransactionResponse();
		}
		catch (Exception e)
		{
			await connection.DestroyAsync(request.TransactionIdentifier);
			return new CommitTransactionResponse { DataException = CreateException(e) };
		}
	}

	/// <summary>
	/// Rolls back a transaction on a database connection.
	/// </summary>
	/// <param name="request">The request parameters.</param>
	/// <param name="context">The server call context.</param>
	/// <returns>An empty response or an exception if an error occurred.</returns>
	public override async Task<RollbackTransactionResponse> RollbackTransaction(RollbackTransactionRequest request, ServerCallContext context)
	{
		if (!clients.TryGetValue(request.ConnectionIdentifier, out var connection))
			return new RollbackTransactionResponse();

		try
		{
			await connection.RollbackAndDestroyAsync(request.TransactionIdentifier);
			return new RollbackTransactionResponse();
		}
		catch (Exception e)
		{
			await connection.DestroyAsync(request.TransactionIdentifier);
			return new RollbackTransactionResponse { DataException = CreateException(e) };
		}
	}

	/// <summary>
	/// Executes a database query and returns the result to the client through the response stream.
	/// </summary>
	/// <param name="request">The request containing the query and parameters.</param>
	/// <param name="responseStream">The response stream to write the result to.</param>
	/// <param name="context">The context of the server call.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public override async Task ExecuteQuery(ExecuteQueryRequest request, IServerStreamWriter<ExecuteQueryResponse> responseStream, ServerCallContext context)
	{
		var command = GetCommand(request);
		if (command == null)
			return;
		try
		{
			await using var reader = await command.ExecuteReaderAsync();
			while (await reader.ReadAsync() && !context.CancellationToken.IsCancellationRequested)
			{
				var row = new ExecuteQueryResponse();
				for (var i = 0; i < reader.FieldCount; i++)
				{
					var field = GetField(reader, i);
					row.Fields.Add(field);
				}

				await responseStream.WriteAsync(row);
			}
		}
		catch (Exception e)
		{
			var result = new ExecuteQueryResponse { DataException = CreateException(e) };
			await responseStream.WriteAsync(result);
		}
	}

	/// <summary>
	/// Executes a database query and returns the result to the client synchronously.
	/// </summary>
	/// <param name="request">The request containing the query and parameters.</param>
	/// <param name="context">The context of the server call.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public override async Task<ExecuteQuerySyncResponse> ExecuteQuerySync(ExecuteQueryRequest request, ServerCallContext context)
	{
		var command = GetCommand(request);
		if (command == null)
			return new ExecuteQuerySyncResponse();
		try
		{
			var result = new ExecuteQuerySyncResponse();
			await using var reader = await command.ExecuteReaderAsync();
			while (await reader.ReadAsync() && !context.CancellationToken.IsCancellationRequested)
			{
				var row = new ExecuteQuerySyncResponseRow();
				for (var i = 0; i < reader.FieldCount; i++)
				{
					var field = GetField(reader, i);
					row.Fields.Add(field);
				}

				result.Rows.Add(row);
			}

			return result;
		}
		catch (Exception e)
		{
			return new ExecuteQuerySyncResponse { DataException = CreateException(e) };
		}
	}


	/// <summary>
	/// Executes a database non query and returns the rows affected.
	/// </summary>
	/// <param name="request">The request containing the query and parameters.</param>
	/// <param name="context">The context of the server call.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public override async Task<ExecuteNonQueryResponse> ExecuteNonQuery(ExecuteQueryRequest request, ServerCallContext context)
	{
		var command = GetCommand(request);
		if (command == null)
			return new ExecuteNonQueryResponse();
		try
		{
			var rowsAffected = await command.ExecuteNonQueryAsync();
			return new ExecuteNonQueryResponse { RowsAffected = rowsAffected };
		}
		catch (Exception e)
		{
			return new ExecuteNonQueryResponse { DataException = CreateException(e) };
		}
	}

	/// <summary>
	/// Gets the command for the given request.
	/// </summary>
	/// <param name="request">The request.</param>
	/// <returns>The command or null otherwise.</returns>
	static DbCommand? GetCommand(ExecuteQueryRequest request)
	{
		if (GetConnection(request) is not { } connection)
			return null;

		if (GetTransaction(request) is not { } transaction)
			return null;
		if (GetCommand(request, connection, transaction) is not { } command)
			return null;
		command.Parameters.Clear();
		foreach (var param in request.Parameters)
		{
			var p = (DataParameter)param;
			AddParameter(command, p.Name, !p.IsNull ? p.Value : null);
		}
		return command;
	}

	/// <summary>
	/// Attempts to retrieve a database transaction associated with the specified connection and transaction identifiers.
	/// </summary>
	/// <param name="request">The request containing the connection and transaction identifiers.</param>
	/// <returns>The transaction associated with the specified identifiers, or null if the connection or transaction cannot be found.</returns>
	static DbTransaction? GetTransaction(ExecuteQueryRequest request)
	{
		return clients.TryGetValue(request.ConnectionIdentifier, out var con) ? con.GetTransaction(request.TransactionIdentifier) : null;
	}

	/// <summary>
	/// Retrieves the command specified by the provided <paramref name="request"/>, <paramref name="connection"/>, and <paramref name="transaction"/>.
	/// </summary>
	/// <param name="request">The <see cref="ExecuteQueryRequest"/> containing the identifier for the command to retrieve.</param>
	/// <param name="connection">The connection associated with the command.</param>
	/// <param name="transaction">The transaction associated with the command.</param>
	/// <returns>The <see cref="DbCommand"/> associated with the specified identifier, or null if it does not exist.</returns>
	static DbCommand? GetCommand(ExecuteQueryRequest request, DbConnection connection, DbTransaction transaction)
	{
		if (!clients.TryGetValue(request.ConnectionIdentifier, out var con))
			return null;

		if (con.GetCommand(request.CommandIdentifier) is not { } result)
			return null;

		result.Connection = connection;
		result.Transaction = transaction;
		result.CommandText = request.Query;
		return result;
	}

	/// <summary>
	/// Adds a parameter to a database command.
	/// </summary>
	/// <param name="command">The command to add the parameter to.</param>
	/// <param name="parameterName">The name of the parameter.</param>
	/// <param name="value">The value of the parameter.</param>
	/// <remarks>
	/// If the parameter name is null or empty, no parameter is added to the command.
	/// </remarks>
	static void AddParameter(IDbCommand command, string parameterName, object? value)
	{
		if (string.IsNullOrEmpty(parameterName)) return;
		var parameter = command.CreateParameter();
		parameter.ParameterName = parameterName;
		parameter.Value = value;

		command.Parameters.Add(parameter);
	}

	/// <summary>
	/// Creates a <see cref="DataException"/> object from an <see cref="Exception"/>.
	/// </summary>
	/// <param name="e">The exception to create a DataException from.</param>
	/// <returns>A <see cref="DataException"/> object containing information from the original exception.</returns>
	static DataException CreateException(Exception e)
	{
		var result = new DataException
		{
			ClassName = e.GetType().FullName ?? "",
			Message = e.Message
		};
		var props = e.GetType()
			.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
		foreach (var prop in props)
		{
			var val = prop.GetValue(e);
			var p = val != null ? new Property { Name = prop.Name, Value = val } : new Property { Name = prop.Name };
			if (!p.IsNull)
				result.Properties.Add(p);
		}

		return result;
	}

	/// <summary>
	/// Gets a <see cref="DbConnection"/> object associated with the specified connection identifier 
	/// from the clients dictionary. Returns null if the connection identifier is not found in the dictionary.
	/// </summary>
	/// <param name="request">The <see cref="ExecuteQueryRequest"/> object representing the request.</param>
	/// <returns>A <see cref="DbConnection"/> object associated with the specified connection identifier,
	/// or null if the connection identifier is not found in the dictionary.</returns>
	static DbConnection? GetConnection(ExecuteQueryRequest request) => clients.TryGetValue(request.ConnectionIdentifier, out var connection) ? connection.Connection : null;

	/// <summary>
	/// Gets the field from a reader with an index.
	/// </summary>
	/// <param name="reader">Reader containing the fields.</param>
	/// <param name="i">Index of field to get.</param>
	/// <returns>The field.</returns>
	static DataField GetField(IDataReader reader, int i)
	{
		return !reader.IsDBNull(i)
			? new DataField { Name = reader.GetName(i), Value = reader[i], DataTypeName = reader.GetDataTypeName(i) }
			: new DataField { Name = reader.GetName(i), DataTypeName = reader.GetDataTypeName(i) };
	}

	/// <summary>
	/// Generates an identifier.
	/// </summary>
	/// <returns>The identifier.</returns>
	static string GenerateIdentifier()
	{
		return Guid.NewGuid().ToString();
	}
}