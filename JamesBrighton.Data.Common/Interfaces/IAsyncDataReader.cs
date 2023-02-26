using System.Data;

namespace JamesBrighton.Data;

public interface IAsyncDataReader : IDataReader, IAsyncDisposable
{
    /// <summary>
    /// Asynchronously closes the data reader. This method and should invoke the method
    /// CloseAsync(System.Threading.CancellationToken)
    /// with CancellationToken.None.
    /// </summary>
    /// <returns>A System.Threading.Tasks.Task representing the asynchronous operation.</returns>
    Task CloseAsync();

    /// <summary>
    /// Asynchronously closes the data reader.
    /// </summary>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>A System.Threading.Tasks.Task representing the asynchronous operation.</returns>
    Task CloseAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously advances the reader to the next record in a result set.
    /// </summary>
    /// <returns>A Task&lt;bool&gt; whose Result property is true if there are more rows or false if there aren't.</returns>
    Task<bool> ReadAsync();

    /// <summary>
    /// Asynchronously advances the reader to the next record in a result set.
    /// </summary>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>A Task&lt;bool&gt; whose Result property is true if there are more rows or false if there aren't.</returns>
    Task<bool> ReadAsync(CancellationToken cancellationToken);
}
