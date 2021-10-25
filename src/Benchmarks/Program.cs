using BenchmarkDotNet.Running;

namespace Benchmarks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<PersistenceRunner>();

            // using var host = JasperHost.For(opts =>
            // {
            //void
            // });
            //
            // var waiter = TargetHandler.WaitForNumber(5, 10.Seconds());
            //
            // var publisher = host.Services.GetRequiredService<ICommandBus>();
            // await publisher.Invoke(Target.Random());
            // await publisher.Invoke(Target.Random());
            // await publisher.Invoke(Target.Random());
            // await publisher.Invoke(Target.Random());
            // await publisher.Invoke(Target.Random());
            //
            // await waiter;


        }
    }
}
