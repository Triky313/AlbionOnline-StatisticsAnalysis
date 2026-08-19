using System.Linq.Expressions;

namespace StatisticsAnalysisTool.Network;

internal static class PacketModelFactory<TPacketModel>
{
    public static Func<Dictionary<byte, object>, TPacketModel> Factory { get; } = CreateFactory();

    private static Func<Dictionary<byte, object>, TPacketModel> CreateFactory()
    {
        Type parametersType = typeof(Dictionary<byte, object>);
        var constructor = typeof(TPacketModel).GetConstructor([parametersType]);

        if (constructor == null)
        {
            throw new InvalidOperationException($"Type {typeof(TPacketModel).FullName} requires a public constructor with a Dictionary<byte, object> parameter.");
        }

        ParameterExpression parameters = Expression.Parameter(parametersType, "parameters");
        NewExpression createModel = Expression.New(constructor, parameters);

        return Expression.Lambda<Func<Dictionary<byte, object>, TPacketModel>>(createModel, parameters).Compile();
    }
}