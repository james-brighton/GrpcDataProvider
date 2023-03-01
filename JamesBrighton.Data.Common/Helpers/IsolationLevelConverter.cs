namespace JamesBrighton.Data.Common.Helpers;

/// <summary>
/// Provides methods to convert isolation levels between System.Data and JamesBrighton.DataProvider.Grpc namespaces.
/// </summary>
public static class IsolationLevelConverter
{
    /// <summary>
    /// Converts a System.Data.IsolationLevel to a JamesBrighton.DataProvider.Grpc.IsolationLevel.
    /// </summary>
    /// <param name="isolationLevel">The System.Data.IsolationLevel to convert.</param>
    /// <returns>The equivalent JamesBrighton.DataProvider.Grpc.IsolationLevel.</returns>
    public static DataProvider.Grpc.IsolationLevel Convert(System.Data.IsolationLevel isolationLevel)
    {
        return isolationLevel switch
        {
            System.Data.IsolationLevel.Unspecified => DataProvider.Grpc.IsolationLevel.Unspecified,
            System.Data.IsolationLevel.Chaos => DataProvider.Grpc.IsolationLevel.Chaos,
            System.Data.IsolationLevel.ReadUncommitted => DataProvider.Grpc.IsolationLevel.ReadUncommitted,
            System.Data.IsolationLevel.ReadCommitted => DataProvider.Grpc.IsolationLevel.ReadCommitted,
            System.Data.IsolationLevel.RepeatableRead => DataProvider.Grpc.IsolationLevel.RepeatableRead,
            System.Data.IsolationLevel.Serializable => DataProvider.Grpc.IsolationLevel.Serializable,
            _ => DataProvider.Grpc.IsolationLevel.Snapshot
        };
    }
    /// <summary>
    /// Converts a JamesBrighton.DataProvider.Grpc.IsolationLevel to a System.Data.IsolationLevel.
    /// </summary>
    /// <param name="isolationLevel">The JamesBrighton.DataProvider.Grpc.IsolationLevel to convert.</param>
    /// <returns>The equivalent System.Data.IsolationLevel.</returns>
    public static System.Data.IsolationLevel Convert(DataProvider.Grpc.IsolationLevel isolationLevel)
    {
        return isolationLevel switch
        {
            DataProvider.Grpc.IsolationLevel.Unspecified => System.Data.IsolationLevel.Unspecified,
            DataProvider.Grpc.IsolationLevel.Chaos => System.Data.IsolationLevel.Chaos,
            DataProvider.Grpc.IsolationLevel.ReadUncommitted => System.Data.IsolationLevel.ReadUncommitted,
            DataProvider.Grpc.IsolationLevel.ReadCommitted => System.Data.IsolationLevel.ReadCommitted,
            DataProvider.Grpc.IsolationLevel.RepeatableRead => System.Data.IsolationLevel.RepeatableRead,
            DataProvider.Grpc.IsolationLevel.Serializable => System.Data.IsolationLevel.Serializable,
            _ => System.Data.IsolationLevel.Snapshot
        };
    }
}