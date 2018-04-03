using Jasper;
using Jasper.Messaging.Runtime.Invocation;

namespace StorytellerSample
{
    public class OtherApp : JasperRegistry
    {
        public OtherApp()
        {
            Handlers.Discovery(x => x.DisableConventionalDiscovery().IncludeType<OtherGuyMessages>());

        }
    }

    public class Query
    {
        public int One { get; set; }
        public int Two { get; set; }
    }

    public class Answer
    {
        public int Sum { get; set; }
        public int Product { get; set; }
    }

    public class OtherGuyMessages
    {
        public object Handle(Query query)
        {
            var answer = new Answer
            {
                Sum = query.One + query.Two,
                Product = query.One * query.Two
            };

            return Respond.With(answer).ToSender();
        }
    }
}
