using Jasper;
using Jasper.Configuration;
using LamarCodeGeneration;
using StorytellerSpecs.Fixtures;

namespace MyApp
{
    // SAMPLE: MyAppRegistryWithOptions
    public class MyAppRegistry : JasperRegistry
    {
        public MyAppRegistry()
        {
            Transports.LightweightListenerAt(2222);

            Publish.Message<Message1>();
            Publish.Message<Message2>();
            Publish.Message<Message3>();

            CodeGeneration.TypeLoadMode = TypeLoadMode.LoadFromPreBuiltAssembly;
        }
    }
    // ENDSAMPLE


    public class MessageHandler
    {
        public void Handle(Message1 msg)
        {
        }

        public void Handle(Message2 msg)
        {
        }

        public void Handle(Message3 msg)
        {
        }

        public void Handle(Message4 msg)
        {
        }

        public void Handle(Message5 msg)
        {
        }
    }
}
