using System;
using Jasper.Configuration;

namespace Jasper
{
    public interface IListenerConfiguration
    {
        /// <summary>
        ///     Specify the maximum number of threads that this worker queue
        ///     can use at one time
        /// </summary>
        /// <param name="maximumParallelHandlers"></param>
        /// <returns></returns>
        IListenerConfiguration MaximumThreads(int maximumParallelHandlers);

        /// <summary>
        ///     Forces this worker queue to use no more than one thread
        /// </summary>
        /// <returns></returns>
        IListenerConfiguration Sequential();

        /// <summary>
        ///     Force any messages enqueued to this worker queue to be durable
        /// </summary>
        /// <returns></returns>
        IListenerConfiguration Durably();

        /// <summary>
        /// By default, messages on this worker queue will not be persisted until
        /// being successfully handled
        /// </summary>
        /// <returns></returns>
        IListenerConfiguration Lightweight();


    }

    public class ListenerConfiguration : IListenerConfiguration
    {
        private readonly Endpoint _endpoint;

        public ListenerConfiguration(Endpoint endpoint)
        {
            _endpoint = endpoint;
            endpoint.IsListener = true;
        }

        IListenerConfiguration IListenerConfiguration.MaximumThreads(int maximumParallelHandlers)
        {
            _endpoint.ExecutionOptions.MaxDegreeOfParallelism = maximumParallelHandlers;
            return this;
        }

        IListenerConfiguration IListenerConfiguration.Sequential()
        {
            _endpoint.ExecutionOptions.MaxDegreeOfParallelism = 1;
            _endpoint.ExecutionOptions.EnsureOrdered = true;
            return this;
        }

        IListenerConfiguration IListenerConfiguration.Durably()
        {
            _endpoint.IsDurable = true;
            return this;
        }

        IListenerConfiguration IListenerConfiguration.Lightweight()
        {
            _endpoint.IsDurable = false;
            return this;
        }
    }


}
