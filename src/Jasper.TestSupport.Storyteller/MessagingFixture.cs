using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using StoryTeller;

namespace Jasper.TestSupport.Storyteller
{
    public abstract class MessagingFixture : Fixture
    {
        /// <summary>
        ///     The service bus for the currently running application
        /// </summary>
        protected IHost Host => Context.Service<IHost>();


        /// <summary>
        ///     Find the
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        protected ExternalNode NodeFor(string nodeName)
        {
            return Context.Service<INodes>().NodeFor(nodeName);
        }
    }
}
