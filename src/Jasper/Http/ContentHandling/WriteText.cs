using System.Collections.Generic;
using Jasper.Codegen;
using Jasper.Http.Model;

namespace Jasper.Http.ContentHandling
{
    public class WriteText : IWriterRule
    {
        public bool Applies(RouteChain chain)
        {
            return chain.ResourceType == typeof(string);
        }

        public IEnumerable<Frame> DetermineWriters(RouteChain chain)
        {
            // TODO -- later, vary this for text/html or other things somehow
            yield return new SetContentType("text/plain");


            var encode = new GetBytes(chain.Action.ReturnVariable);
            yield return encode;

            yield return new WriteTextToResponse(encode);
        }
    }
}