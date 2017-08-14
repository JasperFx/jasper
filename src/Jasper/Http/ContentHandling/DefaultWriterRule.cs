using System;
using System.Collections.Generic;
using Jasper.Codegen;
using Jasper.Conneg;
using Jasper.Http.Model;

namespace Jasper.Http.ContentHandling
{
    public class DefaultWriterRule : IWriterRule
    {
        private readonly SerializationGraph _serializers;

        public DefaultWriterRule(SerializationGraph serializers)
        {
            _serializers = serializers;
        }

        public bool Applies(RouteChain chain)
        {
            return true;
        }

        public IEnumerable<Frame> DetermineWriters(RouteChain chain)
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
    }
}
