namespace JamesBrighton.Data.Common;

/// <summary>
/// Represents a specific date and time in a specific time zone.
/// </summary>
[Serializable]
public struct ZonedDateTimeWrapper
{
    /// <summary>
    /// The specific date and time.
    /// </summary>
    DateTime dateTime;
    /// <summary>
    /// The time zone.
    /// </summary>
    string timeZone;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZonedDateTimeWrapper"/> struct. This constructor only makes sense for serialization.
    /// </summary>
    public ZonedDateTimeWrapper()
    {
        dateTime = new DateTime();
        timeZone = "";
        Offset = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ZonedDateTimeWrapper"/> struct.
    /// </summary>
    /// <param name="dateTime">The specific date and time in UTC.</param>
    /// <param name="timeZone">The time zone.</param>
    /// <param name="offset">The offset from UTC (optional).</param>
#pragma warning disable CS8618
    public ZonedDateTimeWrapper(DateTime dateTime, string timeZone, TimeSpan? offset)
#pragma warning restore CS8618
    {
        if (dateTime.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Value must be in UTC.", nameof(dateTime));
        if (timeZone == null)
            throw new ArgumentNullException(nameof(timeZone));
        if (string.IsNullOrWhiteSpace(timeZone))
            throw new ArgumentException(nameof(timeZone));

        DateTime = dateTime;
        TimeZone = timeZone;
        Offset = offset;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ZonedDateTimeWrapper"/> struct with a null offset.
    /// </summary>
    /// <param name="dateTime">The specific date and time in UTC.</param>
    /// <param name="timeZone">The time zone.</param>
    public ZonedDateTimeWrapper(DateTime dateTime, string timeZone) : this(dateTime, timeZone, null) { }

    /// </summary>
    /// <summary>
    /// Gets or sets the specific date and time.
    /// </summary>
    public DateTime DateTime
    {
        readonly get => dateTime;
        set
        {
            if (value.Kind != DateTimeKind.Utc)
                throw new ArgumentException("Value must be in UTC.", nameof(value));
            dateTime = value;
        }
    }

    /// <summary>
    /// Gets or sets the time zone.
    /// </summary>
    public string TimeZone
    {
        readonly get => timeZone;
        set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(nameof(value));
            timeZone = value;
        }
    }

    /// <summary>
    /// Gets or sets the offset from UTC.
    /// </summary>
    public TimeSpan? Offset { get; set; }

    /// <summary>
    /// Returns a string representation of the ZonedDateTimeWrapper instance.
    /// </summary>
    /// <returns>
    /// A string representation of the ZonedDateTimeWrapper instance, including the date and time, time zone, and offset (if present).
    /// </returns>
    public override readonly string ToString()
    {
        if (Offset == null)
            return $"{DateTime} {TimeZone}";
        return $"{DateTime} {TimeZone} ({Offset})";
    }

    /// <summary>
    /// Determines whether the current ZonedDateTimeWrapper instance is equal to another instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>
    /// true if the current instance is equal to the other instance; otherwise, false.
    /// </returns>
    public override readonly bool Equals(object obj)
    {
        return obj is ZonedDateTimeWrapper zonedDateTimeWrapper && Equals(zonedDateTimeWrapper);
    }

    /// <summary>
    /// Returns a hash code for the current ZonedDateTimeWrapper instance.
    /// </summary>
    /// <returns>
    /// A hash code for the current ZonedDateTimeWrapper instance.
    /// </returns>
    public override readonly int GetHashCode()
    {
        unchecked
        {
            var hash = (int)2166136261;
            hash = (hash * 16777619) ^ DateTime.GetHashCode();
            hash = (hash * 16777619) ^ TimeZone.GetHashCode();
            if (Offset != null)
                hash = (hash * 16777619) ^ Offset.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    /// Determines whether the current ZonedDateTimeWrapper instance is equal to another instance.
    /// </summary>
    /// <param name="other">The ZonedDateTimeWrapper instance to compare with the current instance.</param>
    /// <returns>
    /// true if the current instance is equal to the other instance; otherwise, false.
    /// </returns>
    public readonly bool Equals(ZonedDateTimeWrapper other)
    {
        return DateTime.Equals(other.DateTime) && TimeZone.Equals(other.TimeZone, StringComparison.Ordinal) && ((Offset == null && other.Offset == null) || (Offset != null && other.Offset != null && Offset.Equals(other.Offset)));
    }

    /// <summary>
    /// Determines whether two ZonedDateTimeWrapper instances are equal.
    /// </summary>
    /// <param name="lhs">The first ZonedDateTimeWrapper instance to compare.</param>
    /// <param name="rhs">The second ZonedDateTimeWrapper instance to compare.</param>
    /// <returns>
    /// true if the two instances are equal; otherwise, false.
    /// </returns>
    public static bool operator ==(ZonedDateTimeWrapper lhs, ZonedDateTimeWrapper rhs) => lhs.Equals(rhs);

    /// <summary>
    /// Determines whether two ZonedDateTimeWrapper instances are not equal.
    /// </summary>
    /// <param name="lhs">The first ZonedDateTimeWrapper instance to compare.</param>
    /// <param name="rhs">The second ZonedDateTimeWrapper instance to compare.</param>
    /// <returns>
    /// true if the two instances are not equal; otherwise, false.
    /// </returns>
    public static bool operator !=(ZonedDateTimeWrapper lhs, ZonedDateTimeWrapper rhs) => lhs.Equals(rhs);
}