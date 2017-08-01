using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Baseline;
using Jasper.Codegen;
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
                var frames = rule?.DetermineReaders(chain) ?? jsonReadersFor(chain);
                chain.Middleware.AddRange(frames);
            }

            if (chain.ResourceType != null)
            {
                var rule = _writers.FirstOrDefault(x => x.Applies(chain));
                var frames = rule?.DetermineWriters(chain) ?? jsonWritersFor(chain);

                chain.Postprocessors.AddRange(frames);
            }
        }

        private IEnumerable<Frame> jsonReadersFor(RouteChain chain)
        {
            yield break;
        }

        private IEnumerable<Frame> jsonWritersFor(RouteChain chain)
        {
            yield break;
        }
    }
}
