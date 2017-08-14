using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Codegen;
using Jasper.Codegen.Compilation;
using Jasper.Conneg;
using Jasper.Http.Model;
using Microsoft.AspNetCore.Http;
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

        bool IWriterRule.Applies(RouteChain chain)
        {
            return true;
        }

        IEnumerable<Frame> IReaderRule.DetermineReaders(RouteChain chain)
        {
            if (_serializers.HasMultipleReaders(chain.InputType))
            {
                throw new NotImplementedException();
            }
            else
            {
                chain.Reader = _serializers.JsonReaderFor(chain.InputType);

                yield return new UseReader(chain);
            }
        }

        IEnumerable<Frame> IWriterRule.DetermineWriters(RouteChain chain)
        {
            if (_serializers.HasMultipleWriters(chain.ResourceType))
            {
                throw new NotImplementedException();
            }
            else

            {
                chain.Writer = _serializers.JsonWriterFor(chain.ResourceType);

                yield return new UseWriter(chain);
            }
        }

        bool IReaderRule.Applies(RouteChain chain)
        {
            return true;
        }
    }

    public class StatusCodeWriter : IWriterRule
    {
        public bool Applies(RouteChain chain)
        {
            return chain.ResourceType == typeof(int);
        }

        public IEnumerable<Frame> DetermineWriters(RouteChain chain)
        {
            yield return new SetStatusCode(chain);
        }
    }

    public class SetStatusCode : Frame
    {
        private Variable _response;
        private readonly Variable _return;

        public SetStatusCode(RouteChain chain) : base(false)
        {
            _return = chain.Action.ReturnVariable;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"{_response.Usage}.{nameof(HttpResponse.StatusCode)} = {_return.Usage};");

            Next?.GenerateCode(method, writer);
        }

        protected internal override IEnumerable<Variable> resolveVariables(GeneratedMethod chain)
        {
            yield return _return;

            _response = chain.FindVariable(typeof(HttpResponse));
            yield return _response;
        }
    }
}
