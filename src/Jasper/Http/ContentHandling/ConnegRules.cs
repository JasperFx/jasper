using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Conneg;
using Jasper.Http.Model;

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

            _writers.Add(new WriteJson());
            _readers.Add(new ReadJson());
        }

        public void Apply(RouteChain chain)
        {
            if (chain.InputType != null)
            {
                var rule = _readers.First(x => x.Applies(chain));
                var frames = rule?.DetermineReaders(chain);
                chain.Middleware.AddRange(frames);
            }

            if (chain.ResourceType != null)
            {
                var rule = _writers.First(x => x.Applies(chain));
                var frames = rule?.DetermineWriters(chain);

                chain.Postprocessors.AddRange(frames);
            }
        }
    }
}
