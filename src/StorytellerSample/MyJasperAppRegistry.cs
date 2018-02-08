using Jasper;
using Jasper.Http;
using Jasper.Storyteller;
using StoryTeller;

namespace StorytellerSample
{
    public class MyJasperAppRegistry : JasperHttpRegistry
    {

    }

    // SAMPLE: MyJasperStorytellerHarness
    public class MyJasperStorytellerHarness : JasperStorytellerHost<MyJasperAppRegistry>
    {
        public MyJasperStorytellerHarness()
        {

            // Customize the application by adding testing concerns,
            // extra logging, or maybe override service registrations
            // with stubs
            Registry.EnvironmentName = "Testing";

            Registry.Logging.UseConsoleLogging = true;
        }

        protected override void beforeAll()
        {
            // Runs before any specification are executed one time
            // Perfect place to load any kind of static data

            // Note that you have access to the JasperRuntime
            // of the running application here
            Runtime.Get<ISomeService>().StartUp();
        }

        protected override void afterEach(ISpecContext context)
        {
            // Called immediately after each specification is executed
            Runtime.Get<ISomeService>().CleanUpTestRunData();
        }

        protected override void beforeEach()
        {
            // Called immediately before each specification is executed
            Runtime.Get<ISomeService>().LoadTestingData();
        }

        protected override void afterAll()
        {
            // Called right before shutting down the Storyteller harness
            Runtime.Get<ISomeService>().Shutdown();
        }
    }
    // ENDSAMPLE

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
