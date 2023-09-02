using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Common;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> Services = new ();

    public static void Register<T>(object service)
    {
        Services[typeof(T)] = service;
    }

    public static T Resolve<T>()
    {
        return (T) Services[typeof(T)];
    }

    public static bool IsServiceInDictionary<T>()
    {
        return Services.ContainsKey(typeof(T));
    }
}