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
            _writers.Add(new StatusCodeWriter());
            _writers.Add(new WriteText());


            _writers.Add(new DefaultWriterRule(_serializers));
            _readers.Add(new DefaultReaderRule(_serializers));
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

        protected override IEnumerable<Variable> resolveVariables(GeneratedMethod chain)
        {
            yield return _return;

            _response = chain.FindVariable(typeof(HttpResponse));
            yield return _response;
        }
    }
}
