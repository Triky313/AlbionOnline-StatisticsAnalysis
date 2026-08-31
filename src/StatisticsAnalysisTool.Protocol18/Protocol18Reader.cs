namespace StatisticsAnalysisTool.Protocol18;

internal ref struct Protocol18Reader
{
    private readonly ReadOnlySpan<byte> _source;

    public Protocol18Reader(ReadOnlySpan<byte> source)
    {
        _source = source;
    }

    public int Position { get; private set; }

    public byte ReadByte()
    {
        if (Position >= _source.Length)
        {
            throw new EndOfStreamException("Failed to read a byte from the Protocol18 payload.");
        }

        return _source[Position++];
    }

    public ReadOnlySpan<byte> ReadSpan(int count)
    {
        if (count < 0 || count > _source.Length - Position)
        {
            throw new EndOfStreamException($"Failed to read {count} bytes from the Protocol18 payload.");
        }

        ReadOnlySpan<byte> result = _source.Slice(Position, count);
        Position += count;

        return result;
    }
}