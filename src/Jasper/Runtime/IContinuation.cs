using System;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Transports;

namespace Jasper.Runtime
{
    #region sample_IContinuation
    /// <summary>
    /// Represents an action to take after processing a message
    /// </summary>
    public interface IContinuation
    {
        /// <summary>
        /// Post-message handling action
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="utcNow">The current time</param>
        /// <returns></returns>
        Task Execute(IExecutionContext execution, DateTime utcNow);
    }

    #endregion
}
