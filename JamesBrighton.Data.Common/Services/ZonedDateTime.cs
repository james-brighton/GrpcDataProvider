using FirebirdSql.Data.Types;
using ProtoBuf.Meta;

namespace JamesBrighton.Data.Common;

/// <summary>
/// Represents a specific date and time in a specific time zone.
/// </summary>
[Serializable]
public struct ZonedDateTime
{
    /// <summary>
    /// The time zone.
    /// </summary>
    string timeZone;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZonedDateTime"/> struct. This constructor only makes sense for serialization.
    /// </summary>
    public ZonedDateTime()
    {
        DateTimeInTicks = 0;
        timeZone = "";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ZonedDateTime"/> struct.
    /// </summary>
    /// <param name="dateTimeInTicks">The specific date and time expressed in the number of 100-nanosecond intervals that have elapsed since January 1, 0001 at 00:00:00.000 in the Gregorian calendar.</param>
    /// <param name="timeZone">The time zone.</param>
#pragma warning disable CS8618
    public ZonedDateTime(long dateTimeInTicks, string timeZone)
#pragma warning restore CS8618
    {
        if (timeZone == null)
            throw new ArgumentNullException(nameof(timeZone));
        if (string.IsNullOrWhiteSpace(timeZone))
            throw new ArgumentException(nameof(timeZone));

        DateTimeInTicks = dateTimeInTicks;
        TimeZone = timeZone;
    }

    /// <summary>
    /// A date and time expressed in the number of 100-nanosecond intervals that have elapsed since January 1, 0001 at 00:00:00.000 in the Gregorian calendar.
    /// </summary>
    public long DateTimeInTicks { get; set; }

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
    /// Returns a string representation of the ZonedDateTimeWrapper instance.
    /// </summary>
    /// <returns>
    /// A string representation of the ZonedDateTimeWrapper instance, including the date and time, time zone, and offset (if present).
    /// </returns>
    public override readonly string ToString()
    {
        var dt = new DateTime(DateTimeInTicks, DateTimeKind.Utc);
        return $"{dt} {TimeZone}";
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
        return obj is ZonedDateTime zonedDateTimeWrapper && Equals(zonedDateTimeWrapper);
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
            hash = (hash * 16777619) ^ DateTimeInTicks.GetHashCode();
            hash = (hash * 16777619) ^ TimeZone.GetHashCode();
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
    public readonly bool Equals(ZonedDateTime other) => DateTimeInTicks.Equals(other.DateTimeInTicks) && TimeZone.Equals(other.TimeZone, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two ZonedDateTimeWrapper instances are equal.
    /// </summary>
    /// <param name="lhs">The first ZonedDateTimeWrapper instance to compare.</param>
    /// <param name="rhs">The second ZonedDateTimeWrapper instance to compare.</param>
    /// <returns>
    /// true if the two instances are equal; otherwise, false.
    /// </returns>
    public static bool operator ==(ZonedDateTime lhs, ZonedDateTime rhs) => lhs.Equals(rhs);

    /// <summary>
    /// Determines whether two ZonedDateTimeWrapper instances are not equal.
    /// </summary>
    /// <param name="lhs">The first ZonedDateTimeWrapper instance to compare.</param>
    /// <param name="rhs">The second ZonedDateTimeWrapper instance to compare.</param>
    /// <returns>
    /// true if the two instances are not equal; otherwise, false.
    /// </returns>
    public static bool operator !=(ZonedDateTime lhs, ZonedDateTime rhs) => lhs.Equals(rhs);

    /// <summary>
    /// Registers this class.
    /// </summary>
    public static void RegisterClass()
    {
        RuntimeTypeModel.Default.Add(typeof(ZonedDateTime), applyDefaultBehaviour: true)
            .Add(1, nameof(DateTimeInTicks))
            .Add(2, nameof(TimeZone));

        Serializer2.AddBeforeSerializeConverter<FbZonedDateTime>((o) => 
        {
            var obj = (FbZonedDateTime)o;
            return new ZonedDateTime(obj.DateTime.Ticks, obj.TimeZone);
        });
    }
}