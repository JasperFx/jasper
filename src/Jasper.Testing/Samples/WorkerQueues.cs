using Baseline;
using Jasper.Messaging.WorkerQueues;

namespace Jasper.Testing.Samples
{
    // SAMPLE: AppWithWorkerQueues
    public class AppWithWorkerQueues : JasperRegistry
    {
        public AppWithWorkerQueues()
        {
            // What if you want the StatusUpdated
            // messages to be handled one at a time
            // in the order in which they are received?
            Handlers.Worker("updates")
                .HandlesMessage<StatusUpdated>()
                .Sequential();

            // Super important messages should get more threads
            Handlers.Worker("important")
                .HandlesMessage<SuperImportantMessage>()
                .MaximumParallelization(10); // the default is 5


            // Messages that are ephemeral should not
            // be durable
            Handlers.Worker("fireandforget")
                .HandleMessages(type => type.CanBeCastTo<EphemeralMessage>())
                .IsNotDurable();


            // Force messages assigned to a certain worker queue to be
            // durable
            Handlers.Worker("durable")
                .HandleMessages(x => x.CanBeCastTo<DurableMessage>())
                .IsDurable();

        }
    }
    // ENDSAMPLE

    public class StatusUpdated{}

    public class SuperImportantMessage{}

    public abstract class DurableMessage{}

    public abstract class EphemeralMessage{}

    // SAMPLE: using-WorkerAttribute
    [Worker("important", IsDurable = true)]
    public class MyAppMessage
    {

    }
    // ENDSAMPLE
}
