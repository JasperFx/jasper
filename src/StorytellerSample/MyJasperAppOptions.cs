using Jasper;
using Jasper.Configuration;
using Jasper.TestSupport.Storyteller;
using Microsoft.Extensions.DependencyInjection;
using StoryTeller;

namespace StorytellerSample
{
    public class MyJasperAppOptions : JasperOptions
    {
        public MyJasperAppOptions()
        {
            Services.AddSingleton<IncrementCounter>();
            Endpoints.ListenAtPort(4444);
        }
    }

    // SAMPLE: MyJasperStorytellerHarness
    public class MyJasperStorytellerHarness : JasperStorytellerHost<MyJasperAppOptions>
    {
        public MyJasperStorytellerHarness()
        {
            // Customize the application by adding testing concerns,
            // extra logging, or maybe override service registrations
            // with stubs
            Registry.Services.AddSingleton<ISomeService, ISomeService>();
        }

        protected override void beforeAll()
        {
            // Runs before any specification are executed one time
            // Perfect place to load any kind of static data

            // Note that you have access to the JasperRuntime
            // of the running application here
            Host.Services.GetService<ISomeService>().StartUp();
        }

        protected override void afterEach(ISpecContext context)
        {
            // Called immediately after each specification is executed
            Host.Services.GetService<ISomeService>().CleanUpTestRunData();
        }

        protected override void beforeEach()
        {
            // Called immediately before each specification is executed
            Host.Services.GetService<ISomeService>().LoadTestingData();
        }

        protected override void afterAll()
        {
            // Called right before shutting down the Storyteller harness
            Host.Services.GetService<ISomeService>().Shutdown();
        }
    }
    // ENDSAMPLE

    public class IncrementCounter
    {
        public int Count { get; set; }
    }

    public class Increment
    {
    }

    public class IncrementHandler
    {
        private readonly IncrementCounter _counter;

        public IncrementHandler(IncrementCounter counter)
        {
            _counter = counter;
        }

        public void Handle(Increment increment)
        {
            _counter.Count++;
        }
    }


    public class SomeStuff
    {
        public void Main(string[] args)
        {
            // SAMPLE: running-MyJasperStorytellerHarness
            StorytellerAgent.Run(args, new MyJasperStorytellerHarness());
            // ENDSAMPLE
        }
    }

    public interface ISomeService
    {
        void StartUp();
        void Shutdown();
        void LoadTestingData();
        void CleanUpTestRunData();
    }
}
