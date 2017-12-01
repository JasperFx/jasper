using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Marten.Linq;

namespace Jasper.Marten.Persistence.Resiliency
{
    public class FindOutgoingEnvelopesByDestination : ICompiledListQuery<Envelope>
    {
        public Expression<Func<IQueryable<Envelope>, IEnumerable<Envelope>>> QueryIs()
        {
            return q => q.Where(x => x.OwnerId == OwnerId && x.Status == Status && x.Address == Address).Take(PageSize);
        }

        public string Status { get; set; } = TransportConstants.Outgoing;
        public int OwnerId { get; } = TransportConstants.AnyNode;
        public int PageSize { get; set; } = 100;
        public string Address { get; set; }
    }
}