using System.Data;

namespace JamesBrighton.Data;

/// <summary>
/// Represents an asynchronous database connection.
/// </summary>
public interface IAsyncConnection : IDbConnection, IAsyncDisposable
{
    /// <summary>
    /// Asynchronously closes the connection to the database. This method invokes the virtual method
    /// JamesBrighton.Data.GrpcClient.GrpcDataProvider.GrpcConnection.CloseAsync(System.Threading.CancellationToken)
    /// with CancellationToken.None.
    /// </summary>
    /// <returns>A System.Threading.Tasks.Task representing the asynchronous operation.</returns>
    Task CloseAsync();

    /// <summary>
    /// Asynchronously closes the connection to the database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>A System.Threading.Tasks.Task representing the asynchronous operation.</returns>
    Task CloseAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously begins a database transaction.
    /// </summary>
    /// <returns>
    /// A System.Threading.Tasks.Task that represents the asynchronous operation. The task result contains the
    /// database transaction.
    /// </returns>
    Task<IAsyncDbTransaction> BeginTransactionAsync();

    /// <summary>
    /// Asynchronously begins a database transaction with the specified isolation level.
    /// </summary>
    /// <param name="il">The isolation level of the transaction.</param>
    /// <returns>
    /// A System.Threading.Tasks.Task that represents the asynchronous operation. The task result contains the
    /// database transaction.
    /// </returns>
    Task<IAsyncDbTransaction> BeginTransactionAsync(IsolationLevel il);

    /// <summary>
    /// An asynchronous version of CreateCommand.
    /// </summary>
    /// <returns>A task representing the asynchronous operation with the command in it.</returns>
    Task<IAsyncDbCommand> CreateCommandAsync();

    /// <summary>
    /// An asynchronous version of Open, which opens
    /// a database connection with the settings specified by the ConnectionString.
    /// This method invokes the virtual method
    /// JamesBrighton.Data.GrpcClient.GrpcDataProvider.GrpcConnection.OpenAsync(System.Threading.CancellationToken)
    /// with CancellationToken.None.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task OpenAsync();

    /// <summary>
    /// This is the asynchronous version of Open. Providers
    /// should override with an appropriate implementation. The cancellation token can
    /// optionally be honored. The default implementation invokes the synchronous Open
    /// call and returns a completed task. The default implementation will return a cancelled
    /// task if passed an already cancelled cancellationToken. Exceptions thrown by Open
    /// will be communicated via the returned Task Exception property. Do not invoke
    /// other methods and properties of the connection object until the returned Task
    /// is complete.
    /// </summary>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task OpenAsync(CancellationToken cancellationToken);
}
