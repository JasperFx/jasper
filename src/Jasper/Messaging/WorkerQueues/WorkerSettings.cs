using System;
using System.Collections.Generic;
using System.Linq;

namespace Jasper.Messaging.WorkerQueues
{
    public class WorkerSettings : IListenerSettings
    {
        private readonly IList<Func<Type, bool>> _matches = new List<Func<Type, bool>>();

        public WorkerSettings(string name)
        {
            Name = name;
        }

        // Mostly for informative reasons
        [Obsolete("Eliminate with GH-557")]
        public Uri Uri { get; set; }
        public string Name { get; }

        public int Parallelization { get; set; } = 5;

        public bool IsDurable { get; set; }

        IListenerSettings IListenerSettings.MaximumParallelization(int maximumParallelHandlers)
        {
            Parallelization = maximumParallelHandlers;
            return this;
        }

        IListenerSettings IListenerSettings.Sequential()
        {
            Parallelization = 1;
            return this;
        }

        IListenerSettings IListenerSettings.HandlesMessage<T>()
        {
            _matches.Add(x => x == typeof(T));
            return this;
        }

        [Obsolete("Eliminate with GH-557")]

        IListenerSettings IListenerSettings.HandleMessages(Func<Type, bool> filter)
        {
            _matches.Add(filter);
            return this;
        }

        [Obsolete("Eliminate with GH-557")]

        IListenerSettings IListenerSettings.IsDurable()
        {
            IsDurable = true;
            return this;
        }

        public IListenerSettings IsNotDurable()
        {
            IsDurable = false;
            return this;
        }

        [Obsolete("Eliminate with GH-557")]

        public bool Matches(Type messageType)
        {
            if (!_matches.Any()) return false;

            return _matches.Any(x => x(messageType));
        }
    }
}
