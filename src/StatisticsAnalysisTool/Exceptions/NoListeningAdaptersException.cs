using System;

namespace StatisticsAnalysisTool.Exceptions;

public class NoListeningAdaptersException : Exception
{
    public NoListeningAdaptersException() : base("Error!\nThere are no listening adapters available!")
    {
    }
}