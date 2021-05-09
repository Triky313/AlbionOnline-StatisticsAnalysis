using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class DebugNotificationFragment : LineFragment
    {
        private readonly HandlerType _handlerType;
        private readonly int _handlerCode;

        public DebugNotificationFragment(HandlerType handlerType, int handlerCode, string text)
        {
            _handlerType = handlerType;
            _handlerCode = handlerCode;
            Text = text;
        }
        
        public string Text { get; }
        public string OutputText => $"{GetHandlerName(_handlerType, _handlerCode)}: {Text}";

        public static string GetHandlerName(HandlerType handlerType, int code)
        {
            switch (handlerType)
            {
                case HandlerType.Event:
                    return ((EventCodes)code).ToString();
                case HandlerType.Operation:
                    return ((OperationCodes)code).ToString();
                default:
                    return code.ToString();
            }
        }
    }
}