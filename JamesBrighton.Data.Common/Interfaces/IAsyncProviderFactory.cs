using System.Data;

namespace JamesBrighton.Data;

/// <summary>
/// Represents a factory for creating asynchronous database object.
/// </summary>
public interface IAsyncProviderFactory
{
    /// <summary>
    /// Creates a new instance of an asynchronous database command object.
    /// </summary>
    /// <returns>An <see cref="IAsyncDbCommand"/> object.</returns>
    IAsyncDbCommand CreateCommand();
    /// <summary>
    /// Creates a new instance of an asynchronous database connection object.
    /// </summary>
    /// <returns>An <see cref="IAsyncConnection"/> object.</returns>
    IAsyncConnection CreateConnection();
    /// <summary>
    /// Creates a new instance of a  database connection string builder object.
    /// </summary>
    /// <returns>An <see cref="IConnectionStringBuilder"/> object.</returns>
	IConnectionStringBuilder CreateConnectionStringBuilder();
    /// <summary>
    /// Creates a new instance of an asynchronous database parameter object.
    /// </summary>
    /// <returns>An <see cref="IDbDataParameter"/> object.</returns>
    IDbDataParameter CreateParameter();
    /// <summary>
    /// Creates a new instance of an asynchronous database transaction object.
    /// </summary>
    /// <returns>An <see cref="IAsyncDbTransaction"/> object.</returns>
    IAsyncDbTransaction CreateTransaction();
}
