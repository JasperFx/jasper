using System;
using System.Linq;
using System.Net;
using Baseline;
using Jasper.Util;
using Lamar.Codegen;
using Lamar.IoC.Instances;
using Marten.Util;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Jasper.Testing
{
    public class debugging
    {
        private readonly ITestOutputHelper _output;

        public debugging(ITestOutputHelper output)
        {
            _output = output;
        }


        [Fact]
        public void try_out_resolver_cache()
        {
            using (var runtime = JasperRuntime.Basic())
            {
                runtime.Container.Model.ExportResolverCode<JasperResolverSet>("../../../../Jasper/Internal/Resolvers");
            }
        }

        [Fact]
        public void see_registrations()
        {
            using (var runtime = JasperRuntime.Basic())
            {
                var singletons =
                    runtime.Container.Model.AllInstances;
                        //.Where(x => x.Lifetime == ServiceLifetime.Singleton)
                        //.Where(x => !x.ServiceType.IsOpenGeneric())
                        //.Where(x => x.Instance is GeneratedInstance);

                foreach (var singleton in singletons)
                {
                    _output.WriteLine($"{singleton.ServiceType.GetTypeFullName()} --> {singleton.ImplementationType.FullNameInCode()} --> {singleton.Instance.GetType().Name}");
                }
            }
        }
    }
}
