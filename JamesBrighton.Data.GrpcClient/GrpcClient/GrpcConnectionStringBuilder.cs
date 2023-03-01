using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace JamesBrighton.Data.GrpcClient;

/// <summary>
/// A class for building a gRPC connection string.
/// </summary>
public class GrpcConnectionStringBuilder : IConnectionStringBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GrpcConnectionStringBuilder"/> class.
    /// </summary>
    public GrpcConnectionStringBuilder() => ConnectionString = "";

    /// <summary>
    /// Initializes a new instance of the <see cref="GrpcConnectionStringBuilder"/> class with the specified connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to use.</param>
    public GrpcConnectionStringBuilder(string connectionString) => ConnectionString = connectionString;

    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    public string ConnectionString
    {
        get => ToString();
        set => FromString(value);
    }

    /// <inheritdoc />
    public ICollection<string> Keys => options.Keys;
    /// <inheritdoc />
    public ICollection<string> Values => options.Values;
    /// <inheritdoc />
    public int Count => options.Count;
    /// <inheritdoc />
    public bool IsReadOnly => options.IsReadOnly;

    /// <inheritdoc />
    public string this[string key]
    {
        get => options[key];
        set => options[key] = value;
    }

    /// <inheritdoc />
    public override string ToString() => string.Join(";", options.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => string.Format("{0}={1}", x.Key, WrapValueIfNeeded(x.Value))));

    /// <inheritdoc />
    public void Add(string key, string value) => options.Add(key, value);

    /// <inheritdoc />
    public bool ContainsKey(string key) => options.ContainsKey(key);

    /// <inheritdoc />
    public bool Remove(string key) => options.Remove(key);

    /// <inheritdoc />
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value) => options.TryGetValue(key, out value);

    /// <inheritdoc />
    public void Add(KeyValuePair<string, string> item) =>  options.Add(item.Key, item.Value);

    /// <inheritdoc />
    public void Clear() => options.Clear();

    /// <inheritdoc />
    public bool Contains(KeyValuePair<string, string> item) => options.Contains(new KeyValuePair<string, string>(item.Key, item.Value));

    /// <inheritdoc />
    public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) => options.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public bool Remove(KeyValuePair<string, string> item) => options.Remove(item);

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => options.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => options.GetEnumerator();

    /// <summary>
    /// Loads the options from the specified connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to load.</param>
    void FromString(string connectionString)
    {
        options.Clear();
        const string KeyPairsRegex = "(([\\w\\s\\d]*)\\s*?=\\s*?\"([^\"]*)\"|([\\w\\s\\d]*)\\s*?=\\s*?'([^']*)'|([\\w\\s\\d]*)\\s*?=\\s*?([^\"';][^;]*))";

        if (string.IsNullOrEmpty(connectionString))
            return;

        foreach (var keyPair in Regex.Matches(connectionString, KeyPairsRegex).Cast<Match>())
        {
            if (keyPair.Groups.Count != 8)
                continue;

            var values = new string[]
            {
                (keyPair.Groups[2].Success ? keyPair.Groups[2].Value
                    : keyPair.Groups[4].Success ? keyPair.Groups[4].Value
                        : keyPair.Groups[6].Success ? keyPair.Groups[6].Value
                            : string.Empty)
                .Trim(),
                (keyPair.Groups[3].Success ? keyPair.Groups[3].Value
                    : keyPair.Groups[5].Success ? keyPair.Groups[5].Value
                        : keyPair.Groups[7].Success ? keyPair.Groups[7].Value
                            : string.Empty)
                .Trim()
            };
            if (values.Length != 2 || string.IsNullOrEmpty(values[0]) || string.IsNullOrEmpty(values[1]) || ContainsKey(values[0], StringComparison.OrdinalIgnoreCase))
                continue;
            options.TryAdd(values[0], values[1]);
        }
    }

    /// <summary>
    /// Determines whether the options contains an element with the specified key using the given comparison type.
    /// </summary>
    /// <param name="key">The key to locate in the options.</param>
    /// <param name="comparisonType">One of the enumeration values that specifies the rules for the comparison.</param>
    /// <returns>true if the options contains an element with the key; otherwise, false.</returns>
    bool ContainsKey(string key, StringComparison comparisonType) => options.Any(x => string.Equals(x.Key, key, comparisonType));

    /// <summary>
    /// Wraps the value in quotes if needed.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>The wrapped value, if needed.</returns>
    static string WrapValueIfNeeded(string value) => value.Contains(';') ? "'" + value + "'" : value;

    /// <summary>
    /// The options.
    /// </summary>
    readonly IDictionary<string, string> options = new Dictionary<string, string>();
}
