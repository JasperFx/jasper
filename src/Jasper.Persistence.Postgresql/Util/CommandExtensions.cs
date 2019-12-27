using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Npgsql;
using NpgsqlTypes;

namespace Jasper.Persistence.Postgresql.Util
{
    public static class CommandExtensions
    {


        public static DbCommand With(this DbCommand command, string name, Envelope[] envelopes)
        {
            var parameter = command.CreateParameter().As<NpgsqlParameter>();
            parameter.ParameterName = name;
            parameter.Value = envelopes.Select(x => x.Id).ToArray();
            parameter.NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Uuid;
            command.Parameters.Add(parameter);

            return command;
        }

        public static DbCommand With(this DbCommand command, string name, ErrorReport[] reports)
        {
            var parameter = command.CreateParameter().As<NpgsqlParameter>();
            parameter.ParameterName = name;
            parameter.Value = reports.Select(x => x.Id).ToArray();
            parameter.NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Uuid;
            command.Parameters.Add(parameter);

            return command;
        }

        public static NpgsqlCommand With(this NpgsqlCommand command, string name, object value, NpgsqlDbType dbType)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            parameter.NpgsqlDbType = dbType;
            command.Parameters.Add(parameter);

            return command;
        }

        public static NpgsqlCommand CreateCommand(this NpgsqlConnection conn, string command)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = command;

            return cmd;
        }
    }
}
