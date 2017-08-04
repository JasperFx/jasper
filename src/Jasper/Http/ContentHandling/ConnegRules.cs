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

        public ConnegRules(SerializationGraph serializers, IEnumerable<IReaderRule> readerRules, IEnumerable<IWriterRule> writeRules)
        {
            _serializers = serializers;

            _writers.AddRange(writeRules);
            _readers.AddRange(readerRules);

            addDefaultRules();
        }

        private void addDefaultRules()
        {
            _writers.Add(new WriteText());

            _writers.Add(new WriteJson());
            _readers.Add(new ReadJson());
        }

        public IEnumerable<IWriterRule> Writers => _writers;

        public IEnumerable<IReaderRule> Readers => _readers;

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
