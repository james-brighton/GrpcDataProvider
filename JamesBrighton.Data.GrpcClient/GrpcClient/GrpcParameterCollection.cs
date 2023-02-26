using System.Data;

namespace JamesBrighton.Data.GrpcClient;

/// <summary>
/// Represents a collection of parameters used in a database command in a gRPC implementation of an
/// <see cref="IDbConnection" />.
/// </summary>
public class GrpcParameterCollection : List<GrpcParameter>, IDataParameterCollection
{
    /// <inheritdoc />
    public object this[string parameterName]
    {
        get => this[IndexOf(parameterName)];
        set
        {
            if (value is not GrpcParameter param) return;
            this[IndexOf(parameterName)] = param;
        }
    }

    /// <inheritdoc />
    public bool Contains(string parameterName) => IndexOf(parameterName) >= 0;

    /// <inheritdoc />
    public int IndexOf(string parameterName) =>
        FindIndex(x => string.Equals(x.ParameterName, parameterName, StringComparison.Ordinal));

    /// <inheritdoc />
    public void RemoveAt(string parameterName)
    {
        var i = IndexOf(parameterName);
        if (i < 0)
            return;
        RemoveAt(i);
    }
}