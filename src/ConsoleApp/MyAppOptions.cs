using Jasper;
using LamarCodeGeneration;
using StorytellerSpecs.Fixtures;

namespace MyApp
{
    // SAMPLE: MyAppRegistryWithOptions
    public class MyAppOptions : JasperOptions
    {
        public MyAppOptions()
        {
            Endpoints.ListenAtPort(2222);

            Advanced.CodeGeneration.TypeLoadMode = TypeLoadMode.LoadFromPreBuiltAssembly;
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
