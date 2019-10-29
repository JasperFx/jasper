using Jasper.Configuration;
using Jasper.Persistence.EntityFrameworkCore.Codegen;

namespace Jasper.Persistence.EntityFrameworkCore
{
    /// <summary>
    /// Add to your Jasper application to opt into EF Core-backed
    /// transaction and saga persistence middleware.
    ///
    /// Warning! This has to be used in conjunction with a Jasper
    /// database package
    /// </summary>
    public class EntityFrameworkCoreBackedPersistence : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            var frameProvider = new EFCorePersistenceFrameProvider();
            registry.CodeGeneration.SetSagaPersistence(frameProvider);
            registry.CodeGeneration.SetTransactions(frameProvider);
        }
    }
}
