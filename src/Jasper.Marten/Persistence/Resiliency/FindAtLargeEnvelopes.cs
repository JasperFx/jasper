using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Marten.Linq;

namespace Jasper.Marten.Persistence.Resiliency
{
    // Not 100% sure Marten can handle the Take() in compiled queries, but we'll see
    public class FindAtLargeEnvelopes : ICompiledListQuery<Envelope>
    {
        public Expression<Func<IQueryable<Envelope>, IEnumerable<Envelope>>> QueryIs()
        {
            return q => q.Where(x => x.OwnerId == OwnerId && x.Status == Status).Take(PageSize);
        }

        public string Status { get; set; } = TransportConstants.Incoming;
        public int OwnerId { get; } = TransportConstants.AnyNode;
        public int PageSize { get; set; } = 100;
    }
}
