using FirebirdSql.Data.Types;
using ProtoBuf.Meta;

namespace JamesBrighton.Data.Common;

/// <summary>
/// Represents a specific time span in a specific time zone.
/// </summary>
[Serializable]
public struct ZonedTime
{
    /// <summary>
    /// The time zone.
    /// </summary>
    string timeZone;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZonedTime"/> struct. This constructor only makes sense for serialization.
    /// </summary>
    public ZonedTime()
    {
        TimeSpanInTicks = 0;
        timeZone = "";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ZonedTime"/> struct.
    /// </summary>
    /// <param name="timeSpanInTicks">The specific ticks which have elapsed since midnight.</param>
    /// <param name="timeZone">The time zone.</param>
#pragma warning disable CS8618
    public ZonedTime(long timeSpanInTicks, string timeZone)
#pragma warning restore CS8618
    {
        if (timeZone == null)
            throw new ArgumentNullException(nameof(timeZone));
        if (string.IsNullOrWhiteSpace(timeZone))
            throw new ArgumentException(nameof(timeZone));

        TimeSpanInTicks = timeSpanInTicks;
        TimeZone = timeZone;
    }

    /// <summary>
    /// The specific ticks which have elapsed since midnight.
    /// </summary>
    public long TimeSpanInTicks { get; set; }

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
    /// Returns a string representation of the ZonedTime instance.
    /// </summary>
    /// <returns>
    /// A string representation of the ZonedTime instance, including the date and time, time zone, and offset (if present).
    /// </returns>
    public override readonly string ToString()
    {
        var ts = new TimeSpan(TimeSpanInTicks);
        return $"{ts} {TimeZone}";
    }

    /// <summary>
    /// Determines whether the current ZonedTime instance is equal to another instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>
    /// true if the current instance is equal to the other instance; otherwise, false.
    /// </returns>
    public override readonly bool Equals(object obj)
    {
        return obj is ZonedTime zonedTime && Equals(zonedTime);
    }

    /// <summary>
    /// Returns a hash code for the current ZonedTime instance.
    /// </summary>
    /// <returns>
    /// A hash code for the current ZonedTime instance.
    /// </returns>
    public override readonly int GetHashCode()
    {
        unchecked
        {
            var hash = (int)2166136261;
            hash = (hash * 16777619) ^ TimeSpanInTicks.GetHashCode();
            hash = (hash * 16777619) ^ TimeZone.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    /// Determines whether the current ZonedTime instance is equal to another instance.
    /// </summary>
    /// <param name="other">The ZonedTime instance to compare with the current instance.</param>
    /// <returns>
    /// true if the current instance is equal to the other instance; otherwise, false.
    /// </returns>
    public readonly bool Equals(ZonedTime other) => TimeSpanInTicks.Equals(other.TimeSpanInTicks) && TimeZone.Equals(other.TimeZone, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two ZonedTime instances are equal.
    /// </summary>
    /// <param name="lhs">The first ZonedTime instance to compare.</param>
    /// <param name="rhs">The second ZonedTime instance to compare.</param>
    /// <returns>
    /// true if the two instances are equal; otherwise, false.
    /// </returns>
    public static bool operator ==(ZonedTime lhs, ZonedTime rhs) => lhs.Equals(rhs);

    /// <summary>
    /// Determines whether two ZonedTime instances are not equal.
    /// </summary>
    /// <param name="lhs">The first ZonedTime instance to compare.</param>
    /// <param name="rhs">The second ZonedTime instance to compare.</param>
    /// <returns>
    /// true if the two instances are not equal; otherwise, false.
    /// </returns>
    public static bool operator !=(ZonedTime lhs, ZonedTime rhs) => lhs.Equals(rhs);

    /// <summary>
    /// Registers this class.
    /// </summary>
    public static void RegisterClass()
    {
        RuntimeTypeModel.Default.Add(typeof(ZonedTime), applyDefaultBehaviour: true)
            .Add(1, nameof(TimeSpanInTicks))
            .Add(2, nameof(TimeZone));

        Serializer2.AddBeforeSerializeConverter<FbZonedTime>((o) => 
        {
            var obj = (FbZonedTime)o;
            return new ZonedTime(obj.Time.Ticks, obj.TimeZone);
        });
    }
}