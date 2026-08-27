namespace StatisticsAnalysisTool.PhotonPackageParser;

internal sealed class PhotonFragmentAssembly
{
    private readonly int[] _fragmentOffsets;
    private readonly int[] _fragmentLengths;

    public PhotonFragmentAssembly(int totalLength, int fragmentCount, long timestamp)
    {
        TotalLength = totalLength;
        FragmentCount = fragmentCount;
        TotalPayload = new byte[totalLength];
        _fragmentOffsets = new int[fragmentCount];
        _fragmentLengths = new int[fragmentCount];
        Array.Fill(_fragmentOffsets, -1);
        LastSeenTimestamp = timestamp;
    }

    public int TotalLength { get; }

    public int FragmentCount { get; }

    public int ReceivedFragmentCount { get; private set; }

    public int ReceivedByteCount { get; private set; }

    public int MissingFragmentCount => FragmentCount - ReceivedFragmentCount;

    public byte[] TotalPayload { get; }

    public long LastSeenTimestamp { get; private set; }

    public bool IsComplete => ReceivedFragmentCount == FragmentCount && ReceivedByteCount == TotalLength;

    public bool IsCompatible(int totalLength, int fragmentCount)
    {
        return TotalLength == totalLength && FragmentCount == fragmentCount;
    }

    public bool TryAddFragment(int fragmentNumber, int fragmentOffset, ReadOnlySpan<byte> payload, long timestamp)
    {
        if (fragmentNumber < 0 || fragmentNumber >= FragmentCount || fragmentOffset < 0 || payload.Length <= 0)
        {
            return false;
        }

        if (fragmentOffset > TotalLength || payload.Length > TotalLength - fragmentOffset)
        {
            return false;
        }

        LastSeenTimestamp = timestamp;

        int existingOffset = _fragmentOffsets[fragmentNumber];
        if (existingOffset >= 0)
        {
            return existingOffset == fragmentOffset && _fragmentLengths[fragmentNumber] == payload.Length;
        }

        int fragmentEnd = fragmentOffset + payload.Length;
        if (fragmentNumber > 0)
        {
            int previousOffset = _fragmentOffsets[fragmentNumber - 1];
            if (previousOffset >= 0 && previousOffset + _fragmentLengths[fragmentNumber - 1] > fragmentOffset)
            {
                return false;
            }
        }

        if (fragmentNumber < FragmentCount - 1)
        {
            int nextOffset = _fragmentOffsets[fragmentNumber + 1];
            if (nextOffset >= 0 && fragmentEnd > nextOffset)
            {
                return false;
            }
        }

        payload.CopyTo(TotalPayload.AsSpan(fragmentOffset, payload.Length));
        _fragmentOffsets[fragmentNumber] = fragmentOffset;
        _fragmentLengths[fragmentNumber] = payload.Length;
        ReceivedFragmentCount++;
        ReceivedByteCount += payload.Length;
        return true;
    }
}