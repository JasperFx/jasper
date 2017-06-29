using System;
using System.Linq;
using Baseline;
using Jasper.Bus.Runtime;
using Microsoft.Extensions.Configuration;

namespace Jasper.Bus
{
    public interface IUriLookup
    {
        string Protocol { get; }

        Uri Lookup(Uri original);
    }

}
