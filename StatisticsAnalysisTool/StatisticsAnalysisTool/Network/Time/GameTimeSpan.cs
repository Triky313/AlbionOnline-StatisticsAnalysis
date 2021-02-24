namespace StatisticsAnalysisTool.Network.Time
{
    public readonly struct GameTimeSpan
    {
        public const long TicksPerMillisecond = 10000L;
        public const long TicksPerSecond = 1000 * TicksPerMillisecond;
        public const long TicksPerMinute = 60 * TicksPerSecond;
        public const long TicksPerHour = 60 * TicksPerMinute;
        public const long TicksPerDay = 24 * TicksPerHour;

        public const double TicksPerMillisecondDouble = TicksPerMillisecond;
        public const double TicksPerSecondDouble = TicksPerSecond;
        public const double TicksPerMinuteDouble = TicksPerMinute;
        public const double TicksPerHourDouble = TicksPerHour;
        public const double TicksPerDayDouble = TicksPerDay;

        public readonly long Ticks;

        public long Days => Ticks / TicksPerDay;
        public long Hours => Ticks % TicksPerDay / TicksPerHour;
        public long Minutes => Ticks % TicksPerHour / TicksPerMinute;
        public long Seconds => Ticks % TicksPerMinute / TicksPerSecond;
        public long Milliseconds => Ticks % TicksPerSecond / TicksPerMillisecond;

        public double TotalDays => Ticks / TicksPerDayDouble;
        public double TotalHours => Ticks / TicksPerHourDouble;
        public double TotalMinutes => Ticks / TicksPerMinuteDouble;
        public double TotalSeconds => Ticks / TicksPerSecondDouble;
        public double TotalMilliseconds => Ticks / TicksPerMillisecondDouble;

        public GameTimeSpan(long ticks)
        {
            Ticks = ticks;
        }

        public static GameTimeSpan FromSeconds(double fSeconds)
        {
            return new GameTimeSpan((long)(fSeconds * TicksPerSecond + 0.5));
        }

        public string ToStringVerbose()
        {
            if (Ticks < TicksPerMinute)
            {
                return $"{Seconds:0}.{Milliseconds:000}s";
            }

            if (Ticks < TicksPerHour)
            {
                return $"{Minutes:0}:{Seconds:00}.{Milliseconds:000}";
            }

            return $"{Hours + Days * 24L:0}:{Minutes:00}:{Seconds:00}.{Milliseconds:000}";
        }
    }
}