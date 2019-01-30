using Jasper.Messaging.Durability;

namespace Jasper.Persistence
{
    public static class JasperRuntimeExtensions
    {
        /// <summary>
        ///     Drops and recreates the Sql Server backed persistence database objects
        /// </summary>
        /// <param name="runtime"></param>
        public static void RebuildMessageStorage(this IJasperHost runtime)
        {
            runtime.Get<IEnvelopePersistor>().Admin.RebuildSchemaObjects();
        }
    }
}
