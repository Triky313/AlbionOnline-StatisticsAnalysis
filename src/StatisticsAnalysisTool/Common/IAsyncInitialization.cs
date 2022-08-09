using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common;

/// <summary>
/// Marks a type as requiring asynchronous initialization and provides the result of that initialization.
/// </summary>
public interface IAsyncInitialization
{
    /// <summary>
    /// The result of the asynchronous initialization of this instance.
    /// </summary>
    Task Initialization { get; }
}