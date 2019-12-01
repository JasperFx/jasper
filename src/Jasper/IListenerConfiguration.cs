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
        private readonly Endpoint _settings;

        public ListenerConfiguration(Endpoint settings)
        {
            _settings = settings;
        }

        IListenerConfiguration IListenerConfiguration.MaximumThreads(int maximumParallelHandlers)
        {
            _settings.ExecutionOptions.MaxDegreeOfParallelism = maximumParallelHandlers;
            return this;
        }

        IListenerConfiguration IListenerConfiguration.Sequential()
        {
            _settings.ExecutionOptions.MaxDegreeOfParallelism = 1;
            _settings.ExecutionOptions.EnsureOrdered = true;
            return this;
        }

        IListenerConfiguration IListenerConfiguration.Durably()
        {
            _settings.IsDurable = true;
            return this;
        }

        IListenerConfiguration IListenerConfiguration.Lightweight()
        {
            _settings.IsDurable = false;
            return this;
        }
    }


}
