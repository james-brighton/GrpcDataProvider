namespace JamesBrighton.Data.Common.Helpers;

/// <summary>
/// Provides methods to convert isolation levels between System.Data and Brighton.James.Dataprovider.Grpc namespaces.
/// </summary>
public static class IsolationLevelConverter
{
    /// <summary>
    /// Converts a System.Data.IsolationLevel to a Brighton.James.Dataprovider.Grpc.IsolationLevel.
    /// </summary>
    /// <param name="isolationLevel">The System.Data.IsolationLevel to convert.</param>
    /// <returns>The equivalent Brighton.James.Dataprovider.Grpc.IsolationLevel.</returns>
    public static Brighton.James.Dataprovider.Grpc.IsolationLevel Convert(System.Data.IsolationLevel isolationLevel)
    {
        return isolationLevel switch
        {
            System.Data.IsolationLevel.Unspecified => Brighton.James.Dataprovider.Grpc.IsolationLevel.Unspecified,
            System.Data.IsolationLevel.Chaos => Brighton.James.Dataprovider.Grpc.IsolationLevel.Chaos,
            System.Data.IsolationLevel.ReadUncommitted => Brighton.James.Dataprovider.Grpc.IsolationLevel.ReadUncommitted,
            System.Data.IsolationLevel.ReadCommitted => Brighton.James.Dataprovider.Grpc.IsolationLevel.ReadCommitted,
            System.Data.IsolationLevel.RepeatableRead => Brighton.James.Dataprovider.Grpc.IsolationLevel.RepeatableRead,
            System.Data.IsolationLevel.Serializable => Brighton.James.Dataprovider.Grpc.IsolationLevel.Serializable,
            _ => Brighton.James.Dataprovider.Grpc.IsolationLevel.Snapshot
        };
    }
    /// <summary>
    /// Converts a Brighton.James.Dataprovider.Grpc.IsolationLevel to a System.Data.IsolationLevel.
    /// </summary>
    /// <param name="isolationLevel">The Brighton.James.Dataprovider.Grpc.IsolationLevel to convert.</param>
    /// <returns>The equivalent System.Data.IsolationLevel.</returns>
    public static System.Data.IsolationLevel Convert(Brighton.James.Dataprovider.Grpc.IsolationLevel isolationLevel)
    {
        return isolationLevel switch
        {
            Brighton.James.Dataprovider.Grpc.IsolationLevel.Unspecified => System.Data.IsolationLevel.Unspecified,
            Brighton.James.Dataprovider.Grpc.IsolationLevel.Chaos => System.Data.IsolationLevel.Chaos,
            Brighton.James.Dataprovider.Grpc.IsolationLevel.ReadUncommitted => System.Data.IsolationLevel.ReadUncommitted,
            Brighton.James.Dataprovider.Grpc.IsolationLevel.ReadCommitted => System.Data.IsolationLevel.ReadCommitted,
            Brighton.James.Dataprovider.Grpc.IsolationLevel.RepeatableRead => System.Data.IsolationLevel.RepeatableRead,
            Brighton.James.Dataprovider.Grpc.IsolationLevel.Serializable => System.Data.IsolationLevel.Serializable,
            _ => System.Data.IsolationLevel.Snapshot
        };
    }
}