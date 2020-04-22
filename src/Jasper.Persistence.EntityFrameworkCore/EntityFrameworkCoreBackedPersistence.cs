using Jasper.Configuration;
using Jasper.Persistence.EntityFrameworkCore.Codegen;
using Jasper.Persistence.Sagas;

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
        public void Configure(JasperOptions options)
        {
            var frameProvider = new EFCorePersistenceFrameProvider();
            options.Advanced.CodeGeneration.SetSagaPersistence(frameProvider);
            options.Advanced.CodeGeneration.SetTransactions(frameProvider);
        }
    }

}
