using System;
using System.Collections.Generic;
using Jasper.Codegen;
using Jasper.Conneg;
using Jasper.Http.Model;

namespace Jasper.Http.ContentHandling
{
    public class DefaultReaderRule : IReaderRule
    {
        private readonly SerializationGraph _serializers;

        public DefaultReaderRule(SerializationGraph serializers)
        {
            _serializers = serializers;
        }

        public bool Applies(RouteChain chain)
        {
            return true;
        }

        public IEnumerable<Frame> DetermineReaders(RouteChain chain)
        {
            if (_serializers.HasMultipleReaders(chain.InputType))
            {
                throw new NotImplementedException();
            }
            else
            {
                chain.ReaderType = typeof(NewtonsoftJsonReader<>)
                    .MakeGenericType(chain.InputType);

                yield return new UseReader(chain);
            }


        }
    }
}
