using System;
using System.Threading.Tasks;

namespace Jasper.Messaging.Configuration
{
    /// <summary>
    /// Source of Uri lookups
    /// </summary>
    public interface IUriLookup
    {
        /// <summary>
        /// The scheme or protocol of this lookup. E.g. "config" for configuration lookups
        /// or "consul" for Consul lookups
        /// </summary>
        string Protocol { get; }

        /// <summary>
        /// Retrieve a list of real Uri values for the given aliases
        /// </summary>
        /// <param name="originals"></param>
        /// <returns></returns>
        Task<Uri[]> Lookup(Uri[] originals);
    }
}
