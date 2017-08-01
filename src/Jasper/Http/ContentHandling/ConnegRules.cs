using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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
            if (chain.ResourceType == typeof(string))
            {
                chain.Postprocessors.Add(new SetContentType("text/plain"));

                var encode = new GetBytes(chain.Action.ReturnVariable);
                chain.Postprocessors.Add(encode);

                var writer = new WriteTextToResponse(encode);
                chain.Postprocessors.Add(writer);
            }
        }

        private void determineReader(RouteChain chain)
        {

        }
    }
}
