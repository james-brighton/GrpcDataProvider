namespace JamesBrighton.Data.Common;

/// <summary>
/// This interface presents a (database) parameter.
/// </summary>
public interface IDataParameter
{
    /// <summary>
    /// Gets/sets the name of the parameter.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets/sets the value of the parameter.
    /// </summary>
    object Value { get; set; }

    /// <summary>
    /// Gets a value indicating whether the parameter is empty (null).
    /// </summary>
    bool IsNull { get; }

    /// <summary>
    /// Gets the type of the parameter.
    /// </summary>
    Type Type { get; }
}