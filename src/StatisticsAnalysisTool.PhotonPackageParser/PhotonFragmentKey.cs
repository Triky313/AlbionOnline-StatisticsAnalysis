namespace StatisticsAnalysisTool.PhotonPackageParser;

internal readonly record struct PhotonFragmentKey(short PeerId, int Challenge, byte ChannelId, int StartSequenceNumber);