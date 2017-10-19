﻿using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Conneg;
using Jasper.Http.Model;

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
            try
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
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error trying to apply conneg rules to {chain}",e);
            }
        }

        bool IWriterRule.TryToApply(RouteChain chain)
        {
            var customWriters = _serializers.CustomWritersFor(chain.ResourceType);

            if (customWriters.Length == 1)
            {
                chain.Writer = customWriters.Single();
                chain.Postprocessors.Add(new UseWriter(chain, true));
            }
            else if (customWriters.Length > 1)
            {
                chain.ConnegWriter = _serializers.WriterFor(chain.ResourceType);
                var selectWriter = new SelectWriter();
                chain.Middleware.Add(selectWriter);
                chain.Middleware.Add(new CheckForMissing(406, selectWriter.ReturnVariable));
                chain.Middleware.Add(new UseWriter(chain, false));
            }
            else
            {
                chain.Writer = _serializers.JsonWriterFor(chain.ResourceType);
                chain.Postprocessors.Add(new UseWriter(chain, true));
            }

            return true;
        }

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
                var reader = _serializers.ReaderFor(chain.InputType);


                chain.Reader = _serializers.JsonReaderFor(chain.InputType);
                chain.Middleware.Add(new UseReader(chain, true));
            }

            return true;
        }
    }
}
