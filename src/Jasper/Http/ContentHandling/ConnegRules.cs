using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Baseline;
using Jasper.Conneg;
using Jasper.Http.Model;
using Jasper.Http.Routing;

namespace Jasper.Http.ContentHandling
{
    public class ConnegRules
    {
        private readonly SerializationGraph _serializers;
        private readonly IList<IWriterRule> _writers = new List<IWriterRule>();
        private readonly IList<IReaderRule> _readers = new List<IReaderRule>();

        // TODO -- make the reader and writer rules pluggable
        public ConnegRules(SerializationGraph serializers)
        {
            _serializers = serializers;

            _writers.Add(new WriteText());
        }

        public void Apply(RouteChain chain)
        {
            if (chain.InputType != null)
            {
                var rule = _readers.FirstOrDefault(x => x.Applies(chain));
                if (rule == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(chain), $"Unable to determine a reader strategy for route {chain}");
                }

                chain.Middleware.AddRange(rule.DetermineReaders(chain));
            }

            if (chain.ResourceType != null)
            {
                var rule = _writers.FirstOrDefault(x => x.Applies(chain));
                if (rule == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(chain), $"Unable to determine a writer strategy for route {chain}");
                }

                chain.Postprocessors.AddRange(rule.DetermineWriters(chain));
            }
        }
    }
}
