namespace StatisticsAnalysisTool.Network;

internal sealed class HandlersCollection
{
    private readonly Dictionary<int, IPacketHandler<EventPacket>> _eventHandlers = [];
    private readonly Dictionary<int, IPacketHandler<RequestPacket>> _requestHandlers = [];
    private readonly Dictionary<int, IPacketHandler<ResponsePacket>> _responseHandlers = [];

    public void Add<TPacket>(PacketHandler<TPacket> handler)
    {
        switch (handler)
        {
            case IPacketHandler<EventPacket> eventHandler:
                _eventHandlers[eventHandler.Code] = eventHandler;
                break;
            case IPacketHandler<RequestPacket> requestHandler:
                _requestHandlers[requestHandler.Code] = requestHandler;
                break;
            case IPacketHandler<ResponsePacket> responseHandler:
                _responseHandlers[responseHandler.Code] = responseHandler;
                break;
            default:
                throw new InvalidOperationException($"Packet type {typeof(TPacket).FullName} is not supported.");
        }
    }

    public Task HandleAsync(object request)
    {
        return request switch
        {
            EventPacket eventPacket => HandleAsync(_eventHandlers, eventPacket.EventCode, eventPacket),
            RequestPacket requestPacket => HandleAsync(_requestHandlers, requestPacket.OperationCode, requestPacket),
            ResponsePacket responsePacket => HandleAsync(_responseHandlers, responsePacket.OperationCode, responsePacket),
            _ => Task.CompletedTask
        };
    }

    private static Task HandleAsync<TPacket>(IReadOnlyDictionary<int, IPacketHandler<TPacket>> handlers, int code, TPacket packet)
    {
        return handlers.TryGetValue(code, out IPacketHandler<TPacket> handler) ? handler.HandleAsync(packet) : Task.CompletedTask;
    }
}