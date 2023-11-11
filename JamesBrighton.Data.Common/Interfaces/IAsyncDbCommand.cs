using System.Data;

namespace JamesBrighton.Data;

/// <summary>
/// Represents an asynchronous database command that extends the IDbCommand interface and implements the
/// IAsyncDisposable interface.
/// </summary>
public interface IAsyncDbCommand : IDbCommand, IAsyncDisposable
{
	/// <summary>
	/// An asynchronous version of ExecuteReader, which executes the command against its connection, returning a
	/// IAsyncDataReader
	/// which can be used to access the results. Should invoke ExecuteDataReaderAsync(System.Data.CommandBehavior,
	/// System.Threading.CancellationToken).
	/// </summary>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	Task<IAsyncDataReader> ExecuteReaderAsync(CancellationToken cancellationToken);
	/// <summary>
	/// Invokes ExecuteDataReaderAsync(System.Data.CommandBehavior,System.Threading.CancellationToken).
	/// </summary>
	/// <param name="behavior">One of the enumeration values that specifies the command behavior.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	Task<IAsyncDataReader> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken);
	/// <summary>
	/// An asynchronous version of ExecuteReader, which executes the command against its connection, returning a
	/// IAsyncDataReader
	/// which can be used to access the results. Should invoke
	/// ExecuteDataReaderAsync(System.Data.CommandBehavior,System.Threading.CancellationToken).
	/// </summary>
	/// <param name="behavior">
	/// One of the enumeration values that specifies how the command should execute and
	/// how data should be retrieved.
	/// </param>
	/// <returns>A task representing the asynchronous operation.</returns>
	Task<IAsyncDataReader> ExecuteReaderAsync(CommandBehavior behavior);
	/// <summary>
	/// An asynchronous version of ExecuteReader, which executes the command against its connection, returning a
	/// IAsyncDataReader
	/// which can be used to access the results. Should invoke
	/// ExecuteDataReaderAsync(System.Data.CommandBehavior,System.Threading.CancellationToken)
	/// with CancellationToken.None.
	/// </summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	Task<IAsyncDataReader> ExecuteReaderAsync();
	/// <summary>
	/// Executes the query asynchronously, and returns the first column of the first row in the result set
	/// returned by the query. Extra columns or rows are ignored.
	/// </summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	Task<object?> ExecuteScalarAsync();
	/// <summary>
	/// Executes the query asynchronously, and returns the first column of the first row in the result set
	/// returned by the query. Extra columns or rows are ignored.
	/// </summary>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken);
	/// <summary>
	/// This is the asynchronous version of ExecuteNonQuery.
	/// Providers should override with an appropriate implementation. The cancellation
	/// token may optionally be ignored. The default implementation invokes the synchronous
	/// ExecuteNonQuery method and returns a completed task,
	/// blocking the calling thread. The default implementation will return a cancelled
	/// task if passed an already cancelled cancellation token. Exceptions thrown by
	/// ExecuteNonQuery will be communicated via the returned
	/// Task Exception property. Do not invoke other methods and properties of the
	/// object until the returned Task is complete.
	/// </summary>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken);
	/// <summary>
	/// An asynchronous version of ExecuteNonQuery, which
	/// executes the command against its connection object, returning the number of rows
	/// affected. Should invoke ExecuteNonQueryAsync(System.Threading.CancellationToken)
	/// with CancellationToken.None.
	/// </summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	Task<int> ExecuteNonQueryAsync();
}