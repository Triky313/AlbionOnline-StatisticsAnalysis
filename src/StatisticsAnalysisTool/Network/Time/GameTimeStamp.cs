using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace StatisticsAnalysisTool.Network.Time;

[Serializable]
public struct GameTimeStamp : IComparable<GameTimeStamp>, IEquatable<GameTimeStamp>, IComparable, ISerializable
{
    static GameTimeStamp()
    {
        TimeSource = DefaultTimeSource;
    }

    public GameTimeStamp(long iTicks)
    {
        this = default;
        Ticks = iTicks;
    }

    public static TimeSourceDelegate TimeSource
    {
        get => pTimeSource;
        set => pTimeSource = value;
    }

    public static GameTimeStamp DefaultTimeSource()
    {
        return new GameTimeStamp(DateTime.UtcNow.Ticks);
    }

    public static GameTimeStamp Now => TimeSource?.Invoke() ?? DefaultTimeSource();
    public static GameTimeStamp NextFullHour => GetNextFullHour(Now);
    public static GameTimeStamp NextDayStart => GetNextDayStart(Now);

    public static readonly GameTimeStamp MaxValue = new GameTimeStamp(long.MaxValue);
    public static readonly GameTimeStamp MinValue = new GameTimeStamp(long.MinValue);
    public static readonly GameTimeStamp Zero = new GameTimeStamp(0L);
    public readonly long Ticks;

    [ThreadStatic] private static TimeSourceDelegate pTimeSource;

    public delegate GameTimeStamp TimeSourceDelegate();

    public static GameTimeStamp GetNextFullHour(GameTimeStamp xNow)
    {
        var num = xNow.Ticks % 36000000000L;
        var num2 = xNow.Ticks - num;
        num2 += 36000000000L;
        return new GameTimeStamp(num2);
    }

    public static GameTimeStamp GetNextDayStart(GameTimeStamp xNow)
    {
        var num = xNow.Ticks % 864000000000L;
        var num2 = xNow.Ticks - num;
        num2 += 864000000000L;
        return new GameTimeStamp(num2);
    }

    public static bool operator <(GameTimeStamp a, GameTimeStamp b)
    {
        return a.Ticks < b.Ticks;
    }

    public static bool operator >(GameTimeStamp a, GameTimeStamp b)
    {
        return a.Ticks > b.Ticks;
    }

    public static bool operator ==(GameTimeStamp a, GameTimeStamp b)
    {
        return a.Ticks == b.Ticks;
    }

    public static bool operator !=(GameTimeStamp a, GameTimeStamp b)
    {
        return a.Ticks != b.Ticks;
    }

    public static bool operator <=(GameTimeStamp a, GameTimeStamp b)
    {
        return a.Ticks <= b.Ticks;
    }

    public static bool operator >=(GameTimeStamp a, GameTimeStamp b)
    {
        return a.Ticks >= b.Ticks;
    }

    public static GameTimeStamp operator +(GameTimeStamp a, GameTimeSpan b)
    {
        return new GameTimeStamp(a.Ticks + b.Ticks);
    }

    public static GameTimeStamp operator -(GameTimeStamp a, GameTimeSpan b)
    {
        return new GameTimeStamp(a.Ticks - b.Ticks);
    }

    public static GameTimeSpan operator -(GameTimeStamp a, GameTimeStamp b)
    {
        return new GameTimeSpan(a.Ticks - b.Ticks);
    }

    public static GameTimeStamp Min(GameTimeStamp a, GameTimeStamp b)
    {
        return a < b ? a : b;
    }

    public static GameTimeStamp Max(GameTimeStamp a, GameTimeStamp b)
    {
        return a > b ? a : b;
    }

    public int CompareTo(object obj)
    {
        if (obj is not GameTimeStamp gameTimeStamp)
            throw new ArgumentException();
        if (Ticks > gameTimeStamp.Ticks)
            return 1;
        if (Ticks == gameTimeStamp.Ticks)
            return 0;
        return -1;
    }

    public int CompareTo(GameTimeStamp xOther)
    {
        if (Ticks > xOther.Ticks)
            return 1;
        if (Ticks == xOther.Ticks)
            return 0;
        return -1;
    }

    public override bool Equals(object value)
    {
        if (value is GameTimeStamp gameTimeStamp)
            return gameTimeStamp.Ticks == Ticks;
        throw new ArgumentException();
    }

    public bool Equals(GameTimeStamp other)
    {
        return other.Ticks == Ticks;
    }

    public override int GetHashCode()
    {
        return (int) Ticks;
    }

    public override string ToString()
    {
        return Ticks.ToString();
    }

    public string ToStringVerbose()
    {
        return new GameTimeSpan(Ticks).ToStringVerbose();
    }

    public DateTime ToDateTime()
    {
        return new DateTime(Ticks);
    }

    public static GameTimeStamp FromDateTime(DateTime xDateTime)
    {
        return new GameTimeStamp(xDateTime.Ticks);
    }

    private GameTimeStamp(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException(nameof(info));
        var flag = false;
        var ticks = 0L;
        var enumerator = info.GetEnumerator();
        while (enumerator.MoveNext())
            if (enumerator.Name == "Ticks")
            {
                ticks = Convert.ToInt64(enumerator.Value, CultureInfo.InvariantCulture);
                flag = true;
                break;
            }

        if (!flag)
            throw new SerializationException("Serialization: Missing GameTimeStamp Data");

        Ticks = ticks;
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException(nameof(info));
        info.AddValue("Ticks", Ticks);
    }
}