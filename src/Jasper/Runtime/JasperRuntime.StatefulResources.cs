using System.Collections.Generic;
using System.Linq;
using Oakton.Resources;

namespace Jasper.Runtime;

public partial class JasperRuntime : IStatefulResourceSource
{
    IReadOnlyList<IStatefulResource> IStatefulResourceSource.FindResources()
    {
        var list = new List<IStatefulResource>();
        list.AddRange(Options.OfType<IStatefulResource>());

        return list;
    }
}
