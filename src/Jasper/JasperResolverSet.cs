using System;
using Baseline;
using Lamar.IoC.Exports;
using Lamar.IoC.Instances;

namespace Jasper
{
    internal class CacheResolverAttribute : Attribute
    {

    }

    public class JasperResolverSet : CachedResolverSet
    {
        public override bool Include(GeneratedInstance instance)
        {
            return instance.ServiceType.HasAttribute<CacheResolverAttribute>()
                   || instance.ImplementationType.HasAttribute<CacheResolverAttribute>();
        }
    }
}
