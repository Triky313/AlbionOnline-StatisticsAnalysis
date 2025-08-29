using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace StatisticsAnalysisTool.Diagnostics;

public static class DebugConsole
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FreeConsole();

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool SetConsoleTitle(string lpConsoleTitle);

    private static readonly ConsoleColor ColError = ConsoleColor.Red;
    private static readonly ConsoleColor ColWarn = ConsoleColor.Yellow;
    private static readonly ConsoleColor ColInfo = ConsoleColor.White;
    private static readonly ConsoleColor ColDebug = ConsoleColor.Gray;

    public static string EventHex { get; set; } = "#5196A6";
    public static string OperationRequestHex { get; set; } = "#5196A6";
    public static string OperationResponseHex { get; set; } = "#5196A6";

    private static readonly Lock Lock = new();
    private static volatile bool _attached;
    private static volatile bool _enqOpen;
    private static CancellationTokenSource? _cts;
    private static Task? _writer;

    private static BlockingCollection<(string line, ConsoleColor fallback)> _q = new(new ConcurrentQueue<(string, ConsoleColor)>());

    public static bool Attach(string title = "Debug Console")
    {
        lock (Lock)
        {
            if (_attached) return true;

            _q = new BlockingCollection<(string, ConsoleColor)>(new ConcurrentQueue<(string, ConsoleColor)>());

            if (!AllocConsole()) return false;
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
                Console.InputEncoding = Encoding.UTF8;
                SetConsoleTitle(title);
            }
            catch
            {
                /* ignore */
            }

            EnableAnsi();

            _cts = new CancellationTokenSource();
            _writer = Task.Factory.StartNew(() => WriterLoop(_cts.Token),
                _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            _enqOpen = true;
            _attached = true;
            return true;
        }
    }

    public static void Detach()
    {
        lock (Lock)
        {
            if (!_attached) return;
            _attached = false;
            _enqOpen = false;

            try
            {
                _cts?.Cancel();
                _q.CompleteAdding();
                _writer?.Wait(200);
            }
            catch
            {
                /* ignore */
            }
            finally
            {
                try
                {
                    FreeConsole();
                }
                catch
                {
                    /* ignore */
                }

                _cts?.Dispose();
                _cts = null;
                _writer = null;

                _ansiEnabled = false;

                _q = new BlockingCollection<(string, ConsoleColor)>(new ConcurrentQueue<(string, ConsoleColor)>());
            }
        }
    }

    private static readonly HashSet<int> EventsInclude = [];
    private static readonly HashSet<int> EventsIgnore = [];
    private static readonly HashSet<int> OpsInclude = [];
    private static readonly HashSet<int> OpsIgnore = [];
    private static bool _useIncludeMode;

    /// <summary>
    /// "-events 1,2 -operations 5"  OR  "-events-ignore 3,4 -operations-ignore 0"
    /// Leer: alles.
    /// </summary>
    public static void Configure(string? args)
    {
        lock (Lock)
        {
            EventsInclude.Clear();
            EventsIgnore.Clear();
            OpsInclude.Clear();
            OpsIgnore.Clear();
            _useIncludeMode = false;

            if (string.IsNullOrWhiteSpace(args))
            {
                return;
            }

            var t = Tokenize(args);
            bool hasInc = false, hasIgn = false;

            for (int i = 0; i < t.Count; i++)
            {
                switch (t[i])
                {
                    case "-events":
                        if (HasVal())
                        {
                            AddCsv(t[++i], EventsInclude);
                            hasInc = true;
                        }

                        break;
                    case "-operations":
                        if (HasVal())
                        {
                            AddCsv(t[++i], OpsInclude);
                            hasInc = true;
                        }

                        break;
                    case "-events-ignore":
                        if (HasVal())
                        {
                            AddCsv(t[++i], EventsIgnore);
                            hasIgn = true;
                        }

                        break;
                    case "-operations-ignore":
                        if (HasVal())
                        {
                            AddCsv(t[++i], OpsIgnore);
                            hasIgn = true;
                        }

                        break;
                }

                continue;

                bool HasVal() => i + 1 < t.Count && !t[i + 1].StartsWith("-", StringComparison.Ordinal);
            }

            _useIncludeMode = hasInc && !hasIgn;
            if (hasInc && hasIgn)
            {
                EventsIgnore.Clear();
                OpsIgnore.Clear();
            }
        }
    }

    private static Type? _eventEnumType;
    private static Type? _operationEnumType;

    public static void UseEnums(Type eventEnumType, Type operationEnumType)
    {
        if (eventEnumType is null)
        {
            throw new ArgumentNullException(nameof(eventEnumType));
        }

        if (operationEnumType is null)
        {
            throw new ArgumentNullException(nameof(operationEnumType));
        }

        if (!eventEnumType.IsEnum)
        {
            throw new ArgumentException("Must be an enum type", nameof(eventEnumType));
        }

        if (!operationEnumType.IsEnum)
        {
            throw new ArgumentException("Must be an enum type", nameof(operationEnumType));
        }

        _eventEnumType = eventEnumType;
        _operationEnumType = operationEnumType;
    }

    private static string? GetEnumName(Type? enumType, int id)
    {
        if (enumType is null)
        {
            return null;
        }

        try
        {
            return Enum.IsDefined(enumType, id) ? Enum.GetName(enumType, id) : null;
        }
        catch
        {
            return null;
        }
    }

    public static void LogEvent(byte eventCode, IReadOnlyDictionary<byte, object> parameters)
    {
        if (!_attached)
        {
            return;
        }

        var id = GetIdFromParams(parameters, 252, eventCode);
        if (!PassesFilter(isOp: false, id))
        {
            return;
        }

        var name = GetEnumName(_eventEnumType, id);
        var map = ParamsToMapString(parameters);

        Span<char> time = stackalloc char[19];
        DateTime.Now.TryFormat(time, out _, "s", CultureInfo.InvariantCulture);

        var line = $"[{time}] EVENT [{id}]{name ?? "-"} - map{map}";

        Enq(ColorizeHex(EventHex, line), ColInfo);
    }

    public static void LogOperationRequest(byte opCode, IReadOnlyDictionary<byte, object> parameters)
    {
        if (!_attached)
        {
            return;
        }

        var id = GetIdFromParams(parameters, 253, opCode);
        if (!PassesFilter(isOp: true, id))
        {
            return;
        }

        var name = GetEnumName(_operationEnumType, id);
        var map = ParamsToMapString(parameters);

        Span<char> time = stackalloc char[19];
        DateTime.Now.TryFormat(time, out _, "s", CultureInfo.InvariantCulture);

        var line = $"[{time}] REQUEST [{id}]{name ?? "-"} - map{map}";

        Enq(ColorizeHex(OperationRequestHex, line), ColInfo);
    }

    public static void LogOperationResponse(byte opCode, short returnCode, string debugMessage,
        IReadOnlyDictionary<byte, object> parameters)
    {
        if (!_attached)
        {
            return;
        }

        var id = GetIdFromParams(parameters, 253, opCode);
        if (!PassesFilter(isOp: true, id))
        {
            return;
        }

        var name = GetEnumName(_operationEnumType, id);
        var map = ParamsToMapString(parameters);

        Span<char> time = stackalloc char[19];
        DateTime.Now.TryFormat(time, out _, "s", CultureInfo.InvariantCulture);

        var line = $"[{time}] RESPONSE [{id}]{name ?? "-"} - map{map} | returnCode[{returnCode}] | debugMessage[{debugMessage}]";

        Enq(ColorizeHex(OperationResponseHex, line), ColInfo);
    }

    public static void WriteError(Type? declaringType, Exception? e)
    {
        if (!_attached)
        {
            return;
        }

        var source = declaringType?.ToString() ?? string.Empty;
        var msg = e?.Message ?? "Unknown exception";
        var st = e?.StackTrace;

        Span<char> time = stackalloc char[19];
        DateTime.Now.TryFormat(time, out _, "s", CultureInfo.InvariantCulture);

        Enq($"[{time}] ERROR {source}: {msg}", ColError);

        if (!string.IsNullOrWhiteSpace(st))
        {
            Enq($"{time} [ERROR] {st}", ColError);
        }
    }

    public static void WriteWarn(Type? declaringType, Exception? e)
    {
        if (!_attached)
        {
            return;
        }

        var source = declaringType?.ToString() ?? string.Empty;
        var msg = e?.Message ?? "Warning";

        Span<char> time = stackalloc char[19];
        DateTime.Now.TryFormat(time, out _, "s", CultureInfo.InvariantCulture);

        Enq($"[{time}] WARNING {source}: {msg}", ColWarn);
    }

    public static void WriteInfo(Type? declaringType, Exception? e)
    {
        if (!_attached)
        {
            return;
        }

        var source = declaringType?.ToString() ?? string.Empty;
        var msg = e?.Message ?? "Info";

        Span<char> time = stackalloc char[19];
        DateTime.Now.TryFormat(time, out _, "s", CultureInfo.InvariantCulture);

        Enq($"[{time}] INFO {source}: {msg}", ColInfo);
    }

    public static void WriteInfo(Type? declaringType, string message, string color = "")
    {
        if (!_attached)
        {
            return;
        }

        var source = declaringType?.ToString() ?? string.Empty;

        Span<char> time = stackalloc char[19];
        DateTime.Now.TryFormat(time, out _, "s", CultureInfo.InvariantCulture);

        var line = $"[{time}] INFO {source}: {message}";

        if (string.IsNullOrWhiteSpace(color))
        {
            Enq(line, ColInfo);
            return;
        }

        if (color[0] == '#' && color.Length == 7)
        {
            Enq(ColorizeHex(color, line), ColInfo);
            return;
        }

        if (Enum.TryParse<ConsoleColor>(color, true, out var cc))
        {
            Enq(line, cc);
            return;
        }

        Enq(line, ColInfo);
    }

    public static void WriteDebug(Type? declaringType, Exception? e)
    {
        if (!_attached)
        {
            return;
        }

        var source = declaringType?.ToString() ?? string.Empty;
        var msg = e?.Message ?? "Debug";

        Span<char> time = stackalloc char[19];
        DateTime.Now.TryFormat(time, out _, "s", CultureInfo.InvariantCulture);

        Enq($"{time} DEBUG {source}: {msg}", ColDebug);
    }

    private static int GetIdFromParams(IReadOnlyDictionary<byte, object> parameters, byte key, int fallback)
    {
        if (parameters != null && parameters.TryGetValue(key, out var v))
        {
            try
            {
                return checked((int) Convert.ToInt64(v, CultureInfo.InvariantCulture));
            }
            catch
            {
                // ignored
            }
        }
        return fallback;
    }

    private static string ParamsToMapString(IReadOnlyDictionary<byte, object> p)
    {
        var sb = new StringBuilder();
        sb.Append('[');

        var keys = new List<byte>(p.Keys);
        keys.Sort();

        bool first = true;
        foreach (var k in keys)
        {
            if (!first) sb.Append(' ');
            {
                first = false;
            }

            sb.Append(k.ToString(CultureInfo.InvariantCulture)).Append(':').Append(FormatValue(p[k], 0));
        }

        sb.Append(']');
        return sb.ToString();
    }

    private static string FormatValue(object? v, int depth)
    {
        if (depth > 6)
        {
            return "...";
        }

        if (v is null)
        {
            return "null";
        }

        if (v is string s && TryParseHexPrefixed(s, out var fromHex))
        {
            return FormatBytes(fromHex);
        }

        switch (v)
        {
            case string s2:
                return LooksLikePrintableString(s2) ? s2 : FormatBytes(Encoding.UTF8.GetBytes(s2));
            case bool b:
                return b ? "true" : "false";
            case byte[] ba:
                return FormatBytes(ba);
            case float f:
                return f.ToString("R", CultureInfo.InvariantCulture);
            case double d:
                return d.ToString("R", CultureInfo.InvariantCulture);
            case decimal m:
                return m.ToString(CultureInfo.InvariantCulture);
            case byte or sbyte or short or ushort or int or uint or long or ulong:
                return Convert.ToString(v, CultureInfo.InvariantCulture) ?? "0";
            case System.Collections.IDictionary dct:
                {
                    var entries = new List<(string k, string v)>();
                    foreach (System.Collections.DictionaryEntry de in dct)
                    {
                        var keyStr = Convert.ToString(de.Key, CultureInfo.InvariantCulture) ?? "";
                        entries.Add((keyStr, FormatValue(de.Value, depth + 1)));
                    }

                    entries.Sort((a, b) =>
                    {
                        if (byte.TryParse(a.k, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ka) &&
                            byte.TryParse(b.k, NumberStyles.Integer, CultureInfo.InvariantCulture, out var kb))
                            return ka.CompareTo(kb);
                        return string.CompareOrdinal(a.k, b.k);
                    });

                    var sb = new StringBuilder();
                    sb.Append('{');
                    for (int i = 0; i < entries.Count; i++)
                    {
                        if (i > 0)
                        {
                            sb.Append(' ');
                        }

                        sb.Append(entries[i].k).Append(':').Append(entries[i].v);
                    }

                    sb.Append('}');
                    return sb.ToString();
                }

            case System.Collections.IEnumerable en:
                return FormatEnumerable(en, depth + 1);

            case DateTime dt:
                return dt.ToUniversalTime().ToString("s", CultureInfo.InvariantCulture);
            case DateTimeOffset dto:
                return dto.ToUniversalTime().ToString("s", CultureInfo.InvariantCulture);

            default:
                return v.ToString() ?? "";
        }
    }

    private static bool LooksLikePrintableString(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return true;
        }

        int max = Math.Min(s.Length, 96);
        for (int i = 0; i < max; i++)
        {
            char c = s[i];
            if (c is '\t' or '\n' or '\r')
            {
                continue;
            }

            if (c < 32)
            {
                return false;
            }
        }

        return true;
    }

    private static string FormatEnumerable(System.Collections.IEnumerable en, int depth)
    {
        var raw = new List<string>();
        foreach (var it in en)
        {
            raw.Add(FormatValue(it, depth));
        }

        int start = 0, end = raw.Count - 1;
        while (start <= end && string.IsNullOrWhiteSpace(raw[start]))
        {
            start++;
        }

        while (end >= start && string.IsNullOrWhiteSpace(raw[end]))
        {
            end--;
        }

        if (start > end)
        {
            return "[]";
        }

        var parts = raw.GetRange(start, end - start + 1);
        return "[" + string.Join(' ', parts) + "]";
    }

    private static string FormatBytes(byte[] bytes)
    {
        if (bytes.Length == 0)
        {
            return "[]";
        }

        if (LooksLikeUtf8Printable(bytes))
        {
            return Encoding.UTF8.GetString(bytes);
        }

        if (bytes.Length == 16)
        {
            try
            {
                return new Guid(bytes).ToString();
            }
            catch
            {
                // ignored
            }
        }

        if (bytes.Length == 8)
        {
            try
            {
                return BitConverter.ToInt64(bytes, 0).ToString(CultureInfo.InvariantCulture);
            }
            catch
            {
                // ignored
            }
        }

        return FormatByteArrayAsList(bytes);
    }

    private static string FormatByteArrayAsList(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 0)
        {
            return "[]";
        }

        var sb = new StringBuilder(bytes.Length * 3 + 2);
        sb.Append('[');
        for (int i = 0; i < bytes.Length; i++)
        {
            if (i > 0)
            {
                sb.Append(' ');
            }

            sb.Append(bytes[i].ToString(CultureInfo.InvariantCulture));
        }

        sb.Append(']');
        return sb.ToString();
    }

    private static bool TryParseHexPrefixed(string s, out byte[] bytes)
    {
        bytes = [];
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        var t = s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? s[2..] : s;

        if (t.Length == 0)
        {
            bytes = [];
            return true;
        }

        if ((t.Length & 1) != 0)
        {
            return false;
        }

        foreach (var c in t)
        {
            bool isHex = c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';

            if (!isHex)
            {
                return false;
            }
        }

        try
        {
            bytes = Convert.FromHexString(t);
            return true;
        }
        catch
        {
            bytes = [];
            return false;
        }
    }

    private static bool LooksLikeUtf8Printable(ReadOnlySpan<byte> bytes)
    {
        int max = Math.Min(bytes.Length, 96);
        for (int i = 0; i < max; i++)
        {
            var b = bytes[i];
            if (b is < 9 or 11 or 12)
            {
                return false;
            }

            if (b is > 126 and < 194)
            {
                return false;
            }
        }
        return max > 0;
    }

    private static bool PassesFilter(bool isOp, int id)
    {
        lock (Lock)
        {
            if (_useIncludeMode)
            {
                var inc = isOp ? OpsInclude : EventsInclude;
                return inc.Count == 0 || inc.Contains(id);
            }

            var ign = isOp ? OpsIgnore : EventsIgnore;
            return ign.Count == 0 || !ign.Contains(id);
        }
    }

    private static void Enq(string line, ConsoleColor fallback)
    {
        if (!_enqOpen)
        {
            return;
        }

        var q = _q;
        if (q.IsAddingCompleted)
        {
            return;
        }

        try
        {
            q.TryAdd((line, fallback));
        }
        catch
        {
            /* ignore */
        }
    }

    private static void WriterLoop(CancellationToken ct)
    {
        try
        {
            foreach (var (line, fallback) in _q.GetConsumingEnumerable(ct))
            {
                var prev = Console.ForegroundColor;
                try
                {
                    Console.ForegroundColor = fallback;
                    Console.WriteLine(line);
                }
                finally
                {
                    Console.ForegroundColor = prev;
                }
            }
        }
        catch (OperationCanceledException)
        {
            /* normal */
        }
    }

    private static List<string> Tokenize(string s)
    {
        var res = new List<string>(16);
        var sb = new StringBuilder();
        bool q = false;
        foreach (var ch in s)
        {
            if (ch == '"')
            {
                q = !q;
                continue;
            }

            if (!q && char.IsWhiteSpace(ch))
            {
                if (sb.Length > 0)
                {
                    res.Add(sb.ToString());
                    sb.Clear();
                }
            }
            else
            {
                sb.Append(ch);
            }
        }

        if (sb.Length > 0)
        {
            res.Add(sb.ToString());
        }

        return res;
    }

    private static void AddCsv(string csv, HashSet<int> set)
    {
        var parts = csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var p in parts)
        {
            if (int.TryParse(p, NumberStyles.Integer, CultureInfo.InvariantCulture, out var x))
            {
                set.Add(x);
            }
        }
    }

    private const int StdOutputHandle = -11;
    private const int EnableVirtualTerminalProcessing = 0x0004;

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, int dwMode);

    private static bool _ansiEnabled;

    private static void EnableAnsi()
    {
        _ansiEnabled = false;
        var h = GetStdHandle(StdOutputHandle);
        if (GetConsoleMode(h, out var mode))
        {
            _ansiEnabled = SetConsoleMode(h, mode | EnableVirtualTerminalProcessing);
        }
    }

    private static string ColorizeHex(string hex, string text)
    {
        if (!_ansiEnabled || string.IsNullOrWhiteSpace(hex))
        {
            return text;
        }

        var s = hex[0] == '#' ? hex[1..] : hex;
        if (s.Length != 6 || !int.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb))
        {
            return text;
        }

        int r = (rgb >> 16) & 0xFF, g = (rgb >> 8) & 0xFF, b = rgb & 0xFF;

        return $"\x1b[38;2;{r};{g};{b}m{text}\x1b[0m";
    }
}