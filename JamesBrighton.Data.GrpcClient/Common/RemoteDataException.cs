using JamesBrighton.DataProvider.Grpc;
using JamesBrighton.Data.Common;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace JamesBrighton.Data.GrpcClient.Common;

/// <summary>
/// Represents an exception that occurred while performing a data operation.
/// </summary>
public sealed class RemoteDataException : Exception
{
    readonly List<(string Name, object Value)> properties = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DataException" /> class with the specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RemoteDataException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Gets or sets the class name of the exception.
    /// </summary>
    public string ClassName { get; set; } = "";

    /// <summary>
    /// Gets or sets the property at the specified index.
    /// </summary>
    /// <param name="i">The index of the property to get or set.</param>
    /// <returns>The value of the property at the specified index.</returns>
    public object this[int i]
    {
        get => properties[i].Value;
        set => properties[i] = (properties[i].Name, value);
    }

    /// <summary>
    /// Gets or sets the property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property to get or set.</param>
    /// <returns>The value of the property with the specified name.</returns>
    public object this[string name]
    {
        get
        {
            var index = properties.FindIndex(x => string.Equals(x.Name, name, StringComparison.Ordinal));
            return properties[index].Value;
        }
        set
        {
            var index = properties.FindIndex(x => string.Equals(x.Name, name, StringComparison.Ordinal));
            if (index < 0)
                properties.Add((name, value));
            else
                properties[index] = (name, value);
        }
    }

    /// <summary>
    /// Attempts to retrieve the value of a property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property to retrieve.</param>
    /// <param name="value">
    /// When this method returns, contains the value of the property with the specified name, 
    /// if the property is found; otherwise, <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if a property with the specified name is found; otherwise, <c>false</c>.
    /// </returns>
    public bool TryGetPropertyValue(string name, [MaybeNullWhen(false)] out object value)
    {
        var index = properties.FindIndex(x => string.Equals(x.Name, name, StringComparison.Ordinal));
        if (index < 0)
        {
            value = null;
            return false;
        }

        value = properties[index].Value;
        return true;
    }

    /// <summary>
    /// Throws a data exception from the given data exception.
    /// </summary>
    /// <param name="dataException">The data exception.</param>
    public static void ThrowDataException(DataException dataException)
    {
        var exception = new RemoteDataException(dataException.Message) { ClassName = dataException.ClassName };
        foreach (var prop in dataException.Properties)
        {
            var p = (Property)prop;
            if (!p.IsNull)
                exception[p.Name] = p.Value;
        }

        throw exception;
    }

    /// <summary>
    /// Throws a data exception from the given exception.
    /// </summary>
    /// <param name="e">The exception.</param>
    public static void ThrowDataException(Exception e)
    {
        var exception = new RemoteDataException(e.Message) { ClassName = e.GetType().FullName ?? "" };
        var props = e.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        foreach (var prop in props)
        {
            var val = prop.GetValue(e);
            if (val != null)
                exception[prop.Name] = val;
        }

        throw exception;
    }

    /// <summary>
    /// Gets the number of properties in the exception.
    /// </summary>
    /// <returns>The number of properties in the exception.</returns>
    public int GetPropertyCount() => properties.Count;

    /// <summary>
    /// Gets the index of the property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <returns>The index of the property with the specified name.</returns>
    public int GetPropertyIndex(string name) =>
        properties.FindIndex(x => string.Equals(x.Name, name, StringComparison.Ordinal));

    /// <summary>
    /// Gets the property's name at the specified index.
    /// </summary>
    /// <param name="i">The index of the property to get or set.</param>
    /// <returns>The name of the property at the specified index.</returns>
    public string GetPropertyName(int i) => properties[i].Name;
}