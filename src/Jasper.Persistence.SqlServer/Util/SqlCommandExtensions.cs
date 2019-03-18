using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Baseline;
using Jasper.Messaging.Runtime;
using Jasper.Persistence.Database;

namespace Jasper.Persistence.SqlServer.Util
{
    public static class SqlCommandExtensions
    {
        public static DbCommand WithIdList(this DbCommand cmd, SqlServerSettings settings, Envelope[] envelopes,
            string parameterName = "IDLIST")
        {
            var table = envelopes.BuildIdTable();

            var parameter = cmd.CreateParameter().As<SqlParameter>();
            parameter.ParameterName = parameterName;
            parameter.Value = table;

            parameter.SqlDbType = SqlDbType.Structured;
            parameter.TypeName = $"{settings.SchemaName}.EnvelopeIdList";

            cmd.Parameters.Add(parameter);

            return cmd;
        }
    }
}
