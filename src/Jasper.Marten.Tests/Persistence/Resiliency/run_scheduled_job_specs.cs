using System;
using Jasper.Marten.Tests.Setup;
using Marten;

namespace Jasper.Marten.Tests.Persistence.Resiliency
{
    public class run_scheduled_job_specs : IDisposable
    {
        private JasperRuntime theRuntime;

        public run_scheduled_job_specs()
        {
            theRuntime = JasperRuntime.For(_ =>
            {
                _.Include<MartenBackedPersistence>();
                _.MartenConnectionStringIs(ConnectionSource.ConnectionString);
            });
        }

        public void Dispose()
        {
            theRuntime?.Dispose();
        }


    }


}
