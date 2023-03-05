using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using JamesBrighton.Data.GrpcClient.Common;

namespace JamesBrighton.Data.GrpcClient.Tunnel;

/// <summary>
/// Represents a tunneled command to execute against a database.
/// </summary>
public class TunnelCommand : IAsyncDbCommand
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TunnelCommand" /> class.
	/// </summary>
	public TunnelCommand() { }

	/// <summary>
	/// Initializes a new instance of the <see cref="TunnelCommand" /> class.
	/// </summary>
	/// <param name="connection">Connection to use.</param>
	TunnelCommand(IDbConnection connection)
	{
		Connection = connection;
	}

	/// <inheritdoc />
	[AllowNull]
	public string CommandText
	{
		get => command?.CommandText ?? "";
		set
		{
			if (command == null)
				return;
			command.CommandText = value;
		}
	}

	/// <inheritdoc />
	public int CommandTimeout { get; set; }

	/// <inheritdoc />
	public CommandType CommandType { get; set; }

	/// <inheritdoc />
	public IDbConnection? Connection { get; set; }

	/// <inheritdoc />
	public IDataParameterCollection Parameters { get; } = new ParameterCollection();

	/// <inheritdoc />
	public IDbTransaction? Transaction
	{
		get => transaction;
		set
		{
			transaction = value;
			if (command != null)
				command.Transaction = (transaction as TunnelTransaction)?.Transaction;
		}
	}

	/// <inheritdoc />
	public UpdateRowSource UpdatedRowSource { get; set; }

	/// <inheritdoc />
	public void Cancel()
	{
	}

	/// <inheritdoc />
	public IDbDataParameter CreateParameter() => new Parameter();

	/// <inheritdoc />
	public void Dispose()
	{
		command?.Dispose();
		command = null;
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		await command?.DisposeAsync().AsTask()!;
		command = null;
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc />
	public int ExecuteNonQuery()
	{
		if (Connection == null || command == null)
			throw new InvalidOperationException("There's no connection.");

		CopyParameters();

		try
		{
			return command.ExecuteNonQuery();
		}
		catch (Exception e)
		{
			RemoteDataException.ThrowDataException(e);
			return 0;
		}
	}

	/// <inheritdoc />
	public IDataReader ExecuteReader() => ExecuteReader(CommandBehavior.Default);

	/// <inheritdoc />
	public IDataReader ExecuteReader(CommandBehavior behavior)
	{
		if (Connection == null || command == null)
			throw new InvalidOperationException("There's no connection.");

		CopyParameters();

		try
		{
			var reader = command.ExecuteReader(behavior);
			var result = new TunnelDataReader(reader);
			return result;
		}
		catch (Exception e)
		{
			RemoteDataException.ThrowDataException(e);
			return null!;
		}
	}

	/// <inheritdoc />
	public async Task<IAsyncDataReader> ExecuteReaderAsync(CancellationToken cancellationToken) =>
		await ExecuteReaderAsync(CommandBehavior.Default, CancellationToken.None);

	/// <inheritdoc />
	public async Task<IAsyncDataReader> ExecuteReaderAsync(CommandBehavior behavior,
		CancellationToken cancellationToken)
	{
		if (Connection == null || command == null)
			throw new InvalidOperationException("There's no connection.");

		CopyParameters();

		try
		{
			var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
			var result = new TunnelDataReader(reader);
			return result;
		}
		catch (Exception e)
		{
			RemoteDataException.ThrowDataException(e);
			return null!;
		}
	}

	/// <inheritdoc />
	public async Task<IAsyncDataReader> ExecuteReaderAsync(CommandBehavior behavior) =>
		await ExecuteReaderAsync(behavior, CancellationToken.None);

	/// <inheritdoc />
	public async Task<IAsyncDataReader> ExecuteReaderAsync() => await ExecuteReaderAsync(CommandBehavior.Default);

	/// <inheritdoc />
	public async Task<object?> ExecuteScalarAsync()
	{
		return await ExecuteScalarAsync(CancellationToken.None);
	}

	/// <inheritdoc />
	public async Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken)
	{
		if (Connection == null || command == null)
			throw new InvalidOperationException("There's no connection.");

		CopyParameters();

		try
		{
			return await command.ExecuteScalarAsync(cancellationToken);
		}
		catch (Exception e)
		{
			RemoteDataException.ThrowDataException(e);
			return null;
		}
	}

	/// <inheritdoc />
	public object? ExecuteScalar()
	{
		if (Connection == null || command == null)
			throw new InvalidOperationException("There's no connection.");

		CopyParameters();

		try
		{
			return command.ExecuteScalar();
		}
		catch (Exception e)
		{
			RemoteDataException.ThrowDataException(e);
			return null;
		}
	}

	/// <inheritdoc />
	public void Prepare()
	{
	}

	/// <summary>
	/// Creates a new instance of <see cref="TunnelCommand"/> using the specified gRPC channel and database connection.
	/// </summary>
	/// <param name="connection">The database connection to associate with the command.</param>
	/// <returns>A new instance of <see cref="TunnelCommand"/> with the specified properties.</returns>
	/// <exception cref="InvalidOperationException">Thrown when <paramref name="connection"/> is null or empty.</exception>
	public static TunnelCommand CreateCommand(DbConnection connection)
	{
		if (connection == null)
			throw new InvalidOperationException("There's no connection.");

		var result = new TunnelCommand(connection)
		{
			command = connection.CreateCommand()
		};
		return result;
	}

	/// <summary>
	/// Asynchronously creates a new instance of <see cref="TunnelCommand"/> using the specified gRPC channel and database connection.
	/// </summary>
	/// <param name="connection">The database connection to associate with the command.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains a new instance of <see cref="TunnelCommand"/> with the specified properties.</returns>
	/// <exception cref="InvalidOperationException">Thrown when <paramref name="connection"/> is null or empty.</exception>
	public static async Task<TunnelCommand> CreateCommandAsync(DbConnection connection)
	{
		if (connection == null)
			throw new InvalidOperationException("There's no connection.");

		await Task.Delay(0);
		return new TunnelCommand(connection)
		{
			command = connection.CreateCommand()
		};
	}

	/// <inheritdoc />
	public async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
	{
		if (Connection == null || command == null)
			throw new InvalidOperationException("There's no connection.");

		CopyParameters();

		try
		{
			return await command.ExecuteNonQueryAsync(cancellationToken);
		}
		catch (Exception e)
		{
			RemoteDataException.ThrowDataException(e);
			return 0;
		}
	}

	/// <inheritdoc />
	public async Task<int> ExecuteNonQueryAsync() => await ExecuteNonQueryAsync(CancellationToken.None);

	/// <summary>
	/// Copies the parameters to the command.
	/// </summary>
	void CopyParameters()
	{
		if (command == null)
			return;

		command.Parameters.Clear();
		foreach (var param in Parameters)
		{
			if (param is not Parameter p) continue;
			AddParameter(command, p.ParameterName, p.Value);
		}
	}

	/// <summary>
	/// Adds a parameter.
	/// </summary>
	/// <param name="command">The command to act on.</param>
	/// <param name="parameterName">Name of the parameter.</param>
	/// <param name="value">The value of the parameter.</param>
	static void AddParameter(IDbCommand command, string parameterName, object? value)
	{
		if (string.IsNullOrEmpty(parameterName)) return;
		var parameter = command.CreateParameter();
		parameter.ParameterName = parameterName;
		parameter.Value = value;

		command.Parameters.Add(parameter);
	}
	/// <summary>
	/// The command.
	/// </summary>
	DbCommand? command;
	/// <summary>
	/// The transaction.
	/// </summary>
	IDbTransaction? transaction;
}