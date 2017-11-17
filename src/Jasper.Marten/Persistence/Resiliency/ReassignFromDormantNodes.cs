using System;
using System.Threading.Tasks;
using Marten;

namespace Jasper.Marten.Persistence.Resiliency
{
    public class ReassignFromDormantNodes : IMessagingAction
    {
        public ReassignFromDormantNodes(StoreOptions options)
        {
";
        }

        public Task Execute(IDocumentSession session)
        {
            // think this needs to be a sproc w/ a cursor. Boo.
            throw new NotImplementedException();
        }
    }
}
