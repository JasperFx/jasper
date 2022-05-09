using System.IO;
using System.Reflection;
using Baseline;
using Jasper.Persistence.Database;
using Weasel.Core;
using Weasel.SqlServer.Procedures;

namespace Jasper.Persistence.SqlServer.Schema;

internal class JasperStoredProcedure : StoredProcedure
{
    public JasperStoredProcedure(string fileName, DatabaseSettings settings) : base(
        new DbObjectName(settings.SchemaName, Path.GetFileNameWithoutExtension(fileName)), ReadText(settings, fileName))
    {
    }

    internal static string ReadText(DatabaseSettings databaseSettings, string fileName)
    {
        return Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(typeof(JasperStoredProcedure), fileName)!
            .ReadAllText().Replace("%SCHEMA%", databaseSettings.SchemaName);
    }
}
