namespace Jasper.Bus.Settings
{
    public interface IQueueSettings
    {
        // TODO -- will grow someday to include routing of messages to incoming queues

        IQueueSettings MaximumParallelization(int maximumParallelHandlers    );
        IQueueSettings SingleThreaded();
    }
}