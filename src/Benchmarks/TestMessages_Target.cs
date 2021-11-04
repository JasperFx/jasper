namespace Benchmarks_Generated
{
    public class TestMessages_Target : Jasper.Runtime.Handlers.MessageHandler
    {


        public override System.Threading.Tasks.Task Handle(Jasper.IExecutionContext context, System.Threading.CancellationToken cancellation)
        {
            var target = (TestMessages.Target)context.Envelope.Message;
            Benchmarks.TargetHandler.Handle(target);
            return System.Threading.Tasks.Task.CompletedTask;
        }

    }
}
