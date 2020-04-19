using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Http.Model;
using Jasper.Serialization;

namespace Jasper.Http.ContentHandling
{
    public class ConnegRules : IWriterRule, IReaderRule
    {
        private readonly IList<IReaderRule> _readers = new List<IReaderRule>();
        private readonly HttpSerializationGraph _serializers;
        private readonly IList<IWriterRule> _writers = new List<IWriterRule>();

        public ConnegRules(HttpSerializationGraph serializers, IEnumerable<IReaderRule> readerRules,
            IEnumerable<IWriterRule> writeRules)
        {
            _serializers = serializers;

            _writers.AddRange(writeRules);
            _readers.AddRange(readerRules);

            addDefaultRules();
        }

        public IEnumerable<IWriterRule> Writers => _writers;

        public IEnumerable<IReaderRule> Readers => _readers;

        bool IReaderRule.TryToApply(RouteChain chain)
        {
            var customReaders = _serializers.CustomReadersFor(chain.InputType);

            if (customReaders.Length == 1)
            {
                chain.Reader = customReaders.Single();
                chain.Middleware.Add(new UseReader(chain, true));
            }
            else if (customReaders.Length > 1)
            {
                chain.ConnegReader = _serializers.ReaderFor(chain.InputType);
                var selectReader = new SelectReader();
                chain.Middleware.Add(selectReader);
                chain.Middleware.Add(new CheckForMissing(415, selectReader.ReturnVariable));
                chain.Middleware.Add(new UseReader(chain, false));
            }
            else
            {
                chain.Reader = _serializers.JsonReaderFor(chain.InputType);
                chain.Middleware.Add(new UseReader(chain, true));
            }

            return true;
        }

        bool IWriterRule.TryToApply(RouteChain chain)
        {
            var customWriters = _serializers.CustomWritersFor(chain.ResourceType);

            if (customWriters.Length == 1)
            {
                chain.Writer = customWriters.Single();
                chain.Postprocessors.Add(new UseWriter(chain));
            }
            else if (customWriters.Length > 1)
            {
                chain.ConnegWriter = _serializers.WriterFor(chain.ResourceType);
                var selectWriter = new SelectWriter();
                chain.Middleware.Add(selectWriter);
                chain.Middleware.Add(new CheckForMissing(406, selectWriter.ReturnVariable));
                chain.Middleware.Add(new UseChosenWriter(chain));
            }
            else
            {
                chain.Writer = _serializers.JsonWriterFor(chain.ResourceType);
                chain.Postprocessors.Add(new UseWriter(chain));
            }

            return true;
        }

        private void addDefaultRules()
        {
            _writers.Add(new StatusCodeWriter());
            _writers.Add(new WriteText());


            _writers.Add(this);
            _readers.Add(this);
        }

        public void Apply(RouteChain chain)
        {
            try
            {
                if (chain.InputType != null)
                    foreach (var reader in _readers)
                        if (reader.TryToApply(chain))
                            break;

                if (chain.ResourceType != null)
                    foreach (var writer in _writers)
                        if (writer.TryToApply(chain))
                            break;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error trying to apply conneg rules to {chain}", e);
            }
        }

        public static ConnegRules Empty()
        {
            var graph = new HttpSerializationGraph(
                new ISerializerFactory<IRequestReader, IResponseWriter>[0], new IRequestReader[0],
                new IResponseWriter[0]);
            return new ConnegRules(graph, new IReaderRule[0], new IWriterRule[0]);
        }
    }
}
