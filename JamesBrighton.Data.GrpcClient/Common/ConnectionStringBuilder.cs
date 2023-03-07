using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using static System.FormattableString;

namespace JamesBrighton.Data.GrpcClient.Common;

/// <summary>
/// A class for building a connection string.
/// </summary>
public partial class ConnectionStringBuilder : IConnectionStringBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionStringBuilder"/> class.
    /// </summary>
    public ConnectionStringBuilder() => ConnectionString = "";

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionStringBuilder"/> class with the specified connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to use.</param>
    public ConnectionStringBuilder(string connectionString) => ConnectionString = connectionString;

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
        get => options.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase)).Value ?? "";
        set => options[key] = value;
    }

    /// <inheritdoc />
    public override string ToString() => string.Join(";", options.Select(x => Invariant($"{x.Key}={WrapValue(x.Value)}")));

    /// <inheritdoc />
    public void Add(string key, string value) => options.Add(key, value);

    /// <inheritdoc />
    public bool ContainsKey(string key) => options.ContainsKey(key);

    /// <inheritdoc />
    public bool Remove(string key) => options.Remove(key);

    /// <inheritdoc />
    public bool TryGetValue(string key, out string value) => options.TryGetValue(key, out value);

    /// <inheritdoc />
    public void Add(KeyValuePair<string, string> item) => options.Add(item.Key, item.Value);

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
    /// Converts a ConnectionStringBuilder object into an instance of string.
    /// </summary>
    /// <param name="obj">The ConnectionStringBuilder object to convert.</param>
    /// <returns>An instance of string that represents the ConnectionStringBuilder object.</returns>
    public static implicit operator string(ConnectionStringBuilder obj) => obj.ToString();

    /// <summary>
    /// Loads the options from the specified connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to load.</param>
    void FromString(string connectionString)
    {
        options.Clear();
        if (string.IsNullOrEmpty(connectionString))
            return;

        foreach (var keyPair in new Regex("(([\\w\\s\\d]*)\\s*?=\\s*?\"([^\"]*)\"|([\\w\\s\\d]*)\\s*?=\\s*?'([^']*)'|([\\w\\s\\d]*)\\s*?=\\s*?([^\"';][^;]*))").Matches(connectionString).Cast<Match>())
        {
            if (keyPair.Groups.Count != 8)
                continue;

            var key = FirstMatch(keyPair, new[] { 2, 4, 6 });
            var value = FirstMatch(keyPair, new[] { 3, 5, 7 });
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                continue;
            if (ContainsKey(key, StringComparison.OrdinalIgnoreCase) && string.Equals(key, "provider", StringComparison.OrdinalIgnoreCase))
                continue;

            options[key] = value;
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
    static string WrapValue(string value)
    {
        if (value.StartsWith('\''))
            return "\"" + value + "\"";
        if (value.StartsWith('\"'))
            return "\'" + value + "\'";
        if (value.Contains(';') || value.StartsWith(' ') || value.EndsWith(' '))
            return "'" + value + "'";

        return value;
    }

    /// <summary>
    /// Returns the first matched group value from the provided <paramref name="match"/> object
    /// based on the provided <paramref name="groupNumbers"/> array.
    /// </summary>
    /// <param name="match">The Match object to retrieve the groups from.</param>
    /// <param name="groupNumbers">An enumerable of integers representing the group numbers to retrieve.</param>
    /// <returns>The value of the first successful match in the provided <paramref name="groupNumbers"/> array, or an empty string if none found.</returns>
    static string FirstMatch(Match match, IEnumerable<int> groupNumbers)
    {
        foreach (var groupNumber in groupNumbers)
        {
            if (match.Groups[groupNumber].Success)
                return match.Groups[groupNumber].Value;
        }

        return string.Empty;
    }

    /// <summary>
    /// The options.
    /// </summary>
    readonly IDictionary<string, string> options = new Dictionary<string, string>();
}
