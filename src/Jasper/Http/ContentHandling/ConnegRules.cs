using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Conneg;
using Jasper.Http.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jasper.Http.ContentHandling
{
    public class ConnegRules : IWriterRule, IReaderRule
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
            _writers.Add(new StatusCodeWriter());
            _writers.Add(new WriteText());


            _writers.Add(this);
            _readers.Add(this);
        }

        public IEnumerable<IWriterRule> Writers => _writers;

        public IEnumerable<IReaderRule> Readers => _readers;

        public void Apply(RouteChain chain)
        {
            if (chain.InputType != null)
            {
                foreach (var reader in _readers)
                {
                    if (reader.TryToApply(chain)) break;
                }
            }

            if (chain.ResourceType != null)
            {
                foreach (var writer in _writers)
                {
                    if (writer.TryToApply(chain)) break;
                }
            }
        }

        bool IWriterRule.TryToApply(RouteChain chain)
        {
            if (_serializers.HasMultipleWriters(chain.ResourceType))
            {
                throw new NotImplementedException();
            }
            else

            {
                chain.Writer = _serializers.JsonWriterFor(chain.ResourceType);
                chain.Postprocessors.Add(new UseWriter(chain));
            }

            return true;
        }

        bool IReaderRule.TryToApply(RouteChain chain)
        {
            if (_serializers.HasMultipleReaders(chain.InputType))
            {
                throw new NotImplementedException();
            }
            else
            {
                chain.Reader = _serializers.JsonReaderFor(chain.InputType);
                chain.Middleware.Add(new UseReader(chain));
            }

            return true;
        }
    }
}
