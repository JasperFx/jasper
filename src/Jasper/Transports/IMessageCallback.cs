using System;
using System.Threading.Tasks;

namespace Jasper.Transports
{
    public interface IMessageCallback
    {
        /// <summary>
        /// Mark the message as having been successfully received and processed
        /// </summary>
        /// <returns></returns>
        Task Complete();

        Task MoveToErrors(Exception exception);


        /// <summary>
        /// Requeue the message for later processing
        /// </summary>
        /// <returns></returns>
        Task Defer();

        Task MoveToScheduledUntil(DateTimeOffset time);
    }
}
