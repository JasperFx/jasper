using System.IO;
using Jasper;
using Marten;

namespace Receiver
{
    public static class HomeEndpoint
    {
        public static string Index(JasperRuntime runtime)
        {
            var writer = new StringWriter();
            runtime.Describe(writer);

            return writer.ToString();
        }

        public static void post_marten_clear(IDocumentStore store)
        {
            store.Advanced.Clean.DeleteAllDocuments();
        }
    }
}