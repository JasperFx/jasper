using System;
using System.Threading.Tasks;

namespace Jasper.Bus.Configuration
{
    public interface IUriLookup
    {
        string Protocol { get; }

        Task<Uri[]> Lookup(Uri[] originals);
    }



}
