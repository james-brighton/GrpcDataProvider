using System.Data.Common;

namespace JamesBrighton.Data.GrpcServer.Helpers;

/// <summary>
/// Represents a database connection and provides methods for managing transactions and commands.
/// </summary>
public class DatabaseConnection
{
    /// <summary>
    /// Initializes a new instance of the DatabaseConnection class with the specified DbConnection object.
    /// </summary>
    /// <param name="connection">The DbConnection object used to create the database connection.</param>
    public DatabaseConnection(DbConnection connection) => Connection = connection;

    /// <summary>
    /// Gets the underlying DbConnection object for this DatabaseConnection object.
    /// </summary>
    public DbConnection Connection { get; }

    /// <summary>
    /// Returns the DbCommand object associated with the specified command identifier.
    /// </summary>
    /// <param name="commandIdentifier">The identifier of the command.</param>
    /// <returns>The DbCommand object associated with the specified command identifier, or null if not found.</returns>
    public DbCommand? GetCommand(string commandIdentifier)
    {
        var index = Commands.FindIndex(x =>
            string.Equals(x.CommandIdentifier, commandIdentifier, StringComparison.Ordinal));
        return index >= 0 ? Commands[index].Command : null;
    }

    /// <summary>
    /// Returns the DbTransaction object associated with the specified transaction identifier.
    /// </summary>
    /// <param name="transactionIdentifier">The identifier of the transaction.</param>
    /// <returns>The DbTransaction object associated with the specified transaction identifier, or null if not found.</returns>

    public DbTransaction? GetTransaction(string transactionIdentifier)
    {
        var index = Transactions.FindIndex(x =>
            string.Equals(x.TransactionIdentifier, transactionIdentifier, StringComparison.Ordinal));
        return index >= 0 ? Transactions[index].Transaction : null;
    }

    /// <summary>
    /// Adds a DbCommand object with the specified command identifier to the list of commands.
    /// </summary>
    /// <param name="commandIdentifier">The identifier of the command.</param>
    /// <param name="command">The DbCommand object to add.</param>
    public void AddCommand(string commandIdentifier, DbCommand command) => Commands.Add((commandIdentifier, command));

    /// <summary>
    /// Adds a DbTransaction object with the specified transaction identifier to the list of transactions.
    /// </summary>
    /// <param name="transactionIdentifier">The identifier of the transaction.</param>
    /// <param name="transaction">The DbTransaction object to add.</param>
    public void AddTransaction(string transactionIdentifier, DbTransaction transaction) => Transactions.Add((transactionIdentifier, transaction));

    /// <summary>
    /// Asynchronously destroys the DbCommand object associated with the specified command identifier.
    /// </summary>
    /// <param name="commandIdentifier">The identifier of the command to destroy.</param>
    /// <returns>A System.Threading.Tasks.Task representing the asynchronous operation.</returns>
    public async Task DestroyCommand(string commandIdentifier)
    {
        var i = GetCommandIndex(commandIdentifier);
        if (i < 0)
            return;
        await Commands[i].Command.DisposeAsync();
        Commands.RemoveAt(i);
    }

    /// <summary>
    /// Asynchronously commits the transaction associated with the specified transaction identifier and destroys the associated DbTransaction object.
    /// </summary>
    /// <param name="transactionIdentifier">The identifier of the transaction to commit and destroy.</param>
    /// <returns>A System.Threading.Tasks.Task representing the asynchronous operation.</returns>
    public async Task CommitAndDestroy(string transactionIdentifier)
    {
        var i = GetTransactionIndex(transactionIdentifier);
        if (i < 0)
            return;
        await Transactions[i].Transaction.CommitAsync();
        await Transactions[i].Transaction.DisposeAsync();
        Transactions.RemoveAt(i);
    }

    /// <summary>
    /// Asynchronously rolls back the transaction associated with the specified transaction identifier and destroys the associated DbTransaction object.
    /// </summary>
    /// <param name="transactionIdentifier">The identifier of the transaction to rollback and destroy.</param>
    /// <returns>A System.Threading.Tasks.Task representing the asynchronous operation.</returns>
    public async Task RollbackAndDestroy(string transactionIdentifier)
    {
        var i = GetTransactionIndex(transactionIdentifier);
        if (i < 0)
            return;
        await Transactions[i].Transaction.RollbackAsync();
        await Transactions[i].Transaction.DisposeAsync();
        Transactions.RemoveAt(i);
    }

    /// <summary>
    /// Asynchronously destroys the associated DbTransaction object.
    /// </summary>
    /// <param name="transactionIdentifier">The identifier of the transaction to destroy.</param>
    /// <returns>A System.Threading.Tasks.Task representing the asynchronous operation.</returns>
    public async Task Destroy(string transactionIdentifier)
    {
        var i = GetTransactionIndex(transactionIdentifier);
        if (i < 0)
            return;
        await Transactions[i].Transaction.DisposeAsync();
        Transactions.RemoveAt(i);
    }
    /// <summary>
    /// Returns the index of the transaction with the specified identifier.
    /// </summary>
    /// <param name="transactionIdentifier">The identifier of the transaction.</param>
    /// <returns>The index of the transaction, or -1 if the transaction was not found.</returns>
    int GetTransactionIndex(string transactionIdentifier) => Transactions.FindIndex(x => string.Equals(x.TransactionIdentifier, transactionIdentifier, StringComparison.Ordinal));
    /// <summary>
    /// Returns the index of the command with the specified identifier.
    /// </summary>
    /// <param name="commandIdentifier">The identifier of the command.</param>
    /// <returns>The index of the command, or -1 if the command was not found.</returns>
    int GetCommandIndex(string commandIdentifier) => Commands.FindIndex(x => string.Equals(x.CommandIdentifier, commandIdentifier, StringComparison.Ordinal));

    /// <summary>
    /// The list with command.
    /// </summary>
    List<(string CommandIdentifier, DbCommand Command)> Commands { get; } = new();
    /// <summary>
    /// The list with transactions.
    /// </summary>
    List<(string TransactionIdentifier, DbTransaction Transaction)> Transactions { get; } = new();
}
