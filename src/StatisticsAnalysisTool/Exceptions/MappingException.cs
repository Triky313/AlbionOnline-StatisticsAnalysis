using System;

namespace StatisticsAnalysisTool.Exceptions;

public class MappingException : Exception
{
    public MappingException(string message) : base(message)
    {
    }
}