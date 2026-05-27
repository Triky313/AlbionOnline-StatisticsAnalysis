using System;

namespace StatisticsAnalysisTool.Network;

public class AlbionServerChangedEventArgs : EventArgs
{
    public AlbionServerChangedEventArgs(AlbionServerInfo previousServer, AlbionServerInfo currentServer)
    {
        PreviousServer = previousServer;
        CurrentServer = currentServer;
    }

    public AlbionServerInfo PreviousServer { get; }
    public AlbionServerInfo CurrentServer { get; }
}