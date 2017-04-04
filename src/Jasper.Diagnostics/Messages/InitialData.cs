using System.Collections.Generic;
using System.Linq;
using Jasper.Remotes.Messaging;

namespace Jasper.Diagnostics.Messages
{
    public class InitialData : ClientMessage
    {
        public InitialData(IEnumerable<ChainModel> chains)
            : base("initial-data")
        {
            Chains = chains.ToArray();
        }

        public ChainModel[] Chains { get; }
    }
}