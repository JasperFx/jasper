using Jasper.Conneg;
using Jasper.Http.Model;

namespace Jasper.Http.ContentHandling
{
    public class ConnegRules
    {
        private readonly SerializationGraph _serializers;


        public ConnegRules(SerializationGraph serializers)
        {
            _serializers = serializers;

        }

        public void Apply(RouteChain chain)
        {
            if (chain.InputType != null)
            {
                determineReader(chain);
            }

            if (chain.ResourceType != null)
            {
                determineWriter(chain);
            }
        }

        private void determineWriter(RouteChain chain)
        {
        }

        private void determineReader(RouteChain chain)
        {
        }
    }
}
