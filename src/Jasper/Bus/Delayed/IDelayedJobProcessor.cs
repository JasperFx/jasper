using System;
using System.Threading.Tasks;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.Delayed
{
    public interface IDelayedJobProcessor
    {
        void Enqueue(DateTime executionTime, Envelope envelope);

        Task PlayAll();

        Task PlayAt(DateTime executionTime);

        Task EmptyAll();

        int Count();

        DelayedJob[] QueuedJobs();

        void Start(IHandlerPipeline pipeline, ChannelGraph channels);
    }
}