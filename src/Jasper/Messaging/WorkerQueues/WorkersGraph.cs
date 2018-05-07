using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Messaging.Transports;
using Jasper.Util;

namespace Jasper.Messaging.WorkerQueues
{
    public class WorkersGraph : IWorkersExpression
    {
        private readonly IList<Func<Type, bool>> _durableRules = new List<Func<Type, bool>>();

        private readonly LightweightCache<string, WorkerSettings> _workers
            = new LightweightCache<string, WorkerSettings>(name => new WorkerSettings(name));

        private readonly Dictionary<string, string> _workerAssignments = new Dictionary<string, string>();
        private readonly Dictionary<Type, string> _workerAssignmentsByType = new Dictionary<Type, string>();
        private readonly Dictionary<string, bool> _durableAssignments = new Dictionary<string, bool>();
        private readonly Dictionary<Type, bool> _durableAssignmentsByType = new Dictionary<Type, bool>();


        public WorkersGraph()
        {
            _workers[TransportConstants.Default].Parallelization = 10;
            _workers[TransportConstants.Replies].Parallelization = 5;
            _workers[TransportConstants.Retries].Parallelization = 5;
        }

        public WorkerSettings[] AllWorkers => _workers.ToArray();

        IWorkerSettings IWorkersExpression.Worker(string queueName)
        {
            return _workers[queueName.ToLower()];
        }

        public WorkerSettings this[string queueName] => _workers[queueName.ToLower()];

        public string WorkerFor(Type messageType)
        {
            return _workerAssignmentsByType.ContainsKey(messageType)
                ? _workerAssignmentsByType[messageType]
                : TransportConstants.Default;
        }

        public string WorkerFor(string messageType)
        {
            return _workerAssignments.ContainsKey(messageType)
                ? _workerAssignments[messageType]
                : TransportConstants.Default;
        }

        public bool ShouldBeDurable(string messageType)
        {
            return _durableAssignments.ContainsKey(messageType) && _durableAssignments[messageType];
        }

        public bool ShouldBeDurable(Type messageType)
        {
            return _durableAssignmentsByType.ContainsKey(messageType) && _durableAssignmentsByType[messageType];
        }

        public void Compile(IEnumerable<Type> messageTypes)
        {
            foreach (var messageType in messageTypes)
            {
                analyzeMessageType(messageType);
            }
        }

        private void analyzeMessageType(Type messageType)
        {
            var messageAlias = messageType.ToMessageAlias();
            var alias = messageAlias;



            var worker = AllWorkers.FirstOrDefault(x => x.Matches(messageType));

            bool isDurable = worker?.IsDurable ?? false;

            var workerName = worker?.Name ?? TransportConstants.Default;

            if (messageType.HasAttribute<WorkerAttribute>())
            {
                var att = messageType.GetAttribute<WorkerAttribute>();

                workerName = att.WorkerName;

                worker = _workers[att.WorkerName];
                worker.IsDurable = worker.IsDurable || att.IsDurable;

                if (att.MaximumParallelization > 0)
                {
                    worker.Parallelization = att.MaximumParallelization;
                }

                isDurable = isDurable || worker.IsDurable;
            }

            if (messageType.HasAttribute<DurableAttribute>())
            {
                isDurable = true;
            }

            _workerAssignmentsByType.Add(messageType, workerName);
            _workerAssignments[messageAlias] = workerName;

            _durableAssignments[messageAlias] = isDurable;
            _durableAssignmentsByType[messageType] = isDurable;
        }
    }
}
