using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Messaging.Runtime;

namespace Jasper.Persistence.SqlServer.Util
{
    public static class CommandExtensions
    {
        public static int RunSql(this DbConnection conn, params string[] sqls)
        {
            var sql = sqls.Join(";");
            return conn.CreateCommand().Sql(sql).ExecuteNonQuery();
        }


        public static void AddParameters(this DbCommand command, object parameters)
        {
            if (parameters == null) return;

            var parameterDictionary = parameters.GetType().GetProperties()
                .ToDictionary(x => x.Name, x => x.GetValue(parameters, null));

            foreach (var item in parameterDictionary)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = item.Key;
                parameter.Value = item.Value ?? DBNull.Value;

                command.Parameters.Add(parameter);
            }
        }

        public static DbParameter AddParameter(this DbCommand command, object value, DbType? dbType = null)
        {
            var name = "arg" + command.Parameters.Count;

            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;

            if (dbType.HasValue) parameter.DbType = dbType.Value;

            command.Parameters.Add(parameter);

            return parameter;
        }

        public static DbParameter AddNamedParameter(this DbCommand command, string name, object value,
            DbType? type = null)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;

            if (type.HasValue) parameter.DbType = type.Value;

            command.Parameters.Add(parameter);

            return parameter;
        }

        public static DbCommand With(this DbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);

            return command;
        }

        public static DbCommand With(this DbCommand command, string name, object value, DbType dbType)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            parameter.DbType = dbType;
            command.Parameters.Add(parameter);

            return command;
        }

        public static DbCommand With(this DbCommand command, string name, string value)
        {
            return command.With(name, value, DbType.String);
        }

        public static DbCommand With(this DbCommand command, string name, int value)
        {
            return command.With(name, value, DbType.Int32);
        }

        public static DbCommand With(this DbCommand command, string name, Guid value)
        {
            return command.With(name, value, DbType.Guid);
        }

        public static DbCommand With(this DbCommand command, string name, byte[] value)
        {
            return command.With(name, value, DbType.Binary);
        }

        public static DbCommand With(this DbCommand command, string name, DateTimeOffset? value)
        {
            return command.With(name, value, DbType.DateTimeOffset);
        }



        public static DbCommand Sql(this DbCommand cmd, string sql)
        {
            cmd.CommandText = sql;
            return cmd;
        }


        public static DbCommand CreateCommand(this DbTransaction tx, string command)
        {
            var cmd = tx.Connection.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = command;

            return cmd;
        }

        public static DbCommand CreateCommand(this DbConnection conn, string command)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = command;

            return cmd;
        }

        public static DbCommand CallFunction(this DbConnection conn, string functionName)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = functionName;
            cmd.CommandType = CommandType.StoredProcedure;

            return cmd;
        }

        public static DataTable BuildIdTable(this IEnumerable<Envelope> envelopes)
        {
            var table = new DataTable();
            table.Columns.Add(new DataColumn("ID", typeof(Guid)));
            foreach (var envelope in envelopes) table.Rows.Add(envelope.Id);

            return table;
        }

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


        public static async Task ExecuteOnce(this DbCommand command, CancellationToken cancellation)
        {
            var conn = command.Connection;
            try
            {
                await conn.OpenAsync(cancellation);

                await command.ExecuteNonQueryAsync(cancellation);
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
