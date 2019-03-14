using System;
using System.Collections.Generic;
using System.Data;
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
        public static int RunSql(this SqlConnection conn, params string[] sqls)
        {
            var sql = sqls.Join(";");
            return conn.CreateCommand().Sql(sql).ExecuteNonQuery();
        }


        public static void AddParameters(this SqlCommand command, object parameters)
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

        public static SqlParameter AddParameter(this SqlCommand command, object value, SqlDbType? dbType = null)
        {
            var name = "arg" + command.Parameters.Count;

            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;

            if (dbType.HasValue) parameter.SqlDbType = dbType.Value;

            command.Parameters.Add(parameter);

            return parameter;
        }

        public static SqlParameter AddNamedParameter(this SqlCommand command, string name, object value,
            SqlDbType? type = null)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;

            if (type.HasValue) parameter.SqlDbType = type.Value;

            command.Parameters.Add(parameter);

            return parameter;
        }

        public static SqlCommand With(this SqlCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);

            return command;
        }

        public static SqlCommand With(this SqlCommand command, string name, object value, SqlDbType dbType,
            string parameterTypeName = null)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            parameter.SqlDbType = dbType;
            command.Parameters.Add(parameter);

            if (parameterTypeName.IsNotEmpty()) parameter.TypeName = parameterTypeName;

            return command;
        }

        public static SqlCommand AsSproc(this SqlCommand command)
        {
            command.CommandType = CommandType.StoredProcedure;

            return command;
        }

        public static SqlCommand Sql(this SqlCommand cmd, string sql)
        {
            cmd.CommandText = sql;
            return cmd;
        }

        public static SqlCommand CallsSproc(this SqlCommand cmd, DbObjectName function)
        {
            if (cmd == null) throw new ArgumentNullException(nameof(cmd));
            if (function == null) throw new ArgumentNullException(nameof(function));

            cmd.CommandText = function.QualifiedName;
            cmd.CommandType = CommandType.StoredProcedure;

            return cmd;
        }

        public static SqlCommand Returns(this SqlCommand command, string name, SqlDbType type)
        {
            var parameter = command.AddParameter(name);
            parameter.SqlDbType = type;
            parameter.Direction = ParameterDirection.ReturnValue;
            return command;
        }

        public static SqlCommand WithText(this SqlCommand command, string sql)
        {
            command.CommandText = sql;
            return command;
        }

        public static SqlCommand CreateCommand(this SqlConnection conn, SqlTransaction tx, string command)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = command;

            return cmd;
        }

        public static SqlCommand CreateCommand(this SqlTransaction tx, string command)
        {
            var cmd = tx.Connection.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = command;

            return cmd;
        }

        public static SqlCommand CreateCommand(this SqlConnection conn, string command)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = command;

            return cmd;
        }

        public static SqlCommand CallFunction(this SqlConnection conn, string functionName)
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

        public static SqlCommand WithIdList(this SqlCommand cmd, SqlServerSettings settings, Envelope[] envelopes,
            string parameterName = "IDLIST")
        {
            var table = envelopes.BuildIdTable();

            var list = cmd.Parameters.AddWithValue(parameterName, table);
            list.SqlDbType = SqlDbType.Structured;
            list.TypeName = $"{settings.SchemaName}.EnvelopeIdList";

            return cmd;
        }


        public static async Task ExecuteOnce(this SqlCommand command, CancellationToken cancellation)
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
