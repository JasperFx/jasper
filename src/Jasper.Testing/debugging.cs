using System.Data.SqlClient;
using Jasper.Messaging.ErrorHandling;
using Jasper.Messaging.Runtime.Invocation;
using Marten.Util;
using Xunit;
using Xunit.Abstractions;
using LamarCompiler;
using Polly;

namespace Jasper.Testing
{
    public class debugging
    {
        public debugging(ITestOutputHelper output)
        {
            _output = output;
        }

        private readonly ITestOutputHelper _output;

        [Fact]
        public void see_registrations()
        {
            using (var runtime = JasperHost.Basic())
            {
                var singletons =
                    runtime.Container.Model.AllInstances;
                //.Where(x => x.Lifetime == ServiceLifetime.Singleton)
                //.Where(x => !x.ServiceType.IsOpenGeneric())
                //.Where(x => x.Instance is GeneratedInstance);

                foreach (var singleton in singletons)
                    _output.WriteLine(
                        $"{singleton.ServiceType.GetTypeFullName()} --> {singleton.ImplementationType.FullNameInCode()} --> {singleton.Instance.GetType().Name}");
            }
        }

        [Fact]
        public void playing_with_polly()
        {
            var builder = Policy<IContinuation>.Handle<SqlException>();


        }

    }
}
