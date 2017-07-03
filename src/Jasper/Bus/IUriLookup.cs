using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Runtime;
using Microsoft.Extensions.Configuration;

namespace Jasper.Bus
{
    public interface IUriLookup
    {
        string Protocol { get; }

        Task<Uri[]> Lookup(Uri[] originals);
    }

}
