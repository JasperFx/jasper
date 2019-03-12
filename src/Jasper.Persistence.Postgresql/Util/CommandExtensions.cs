using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Messaging.Runtime;
using Npgsql;
using NpgsqlTypes;

namespace Jasper.Persistence.Postgresql.Util
{
    public static class CommandExtensions
    {
        public static async Task<Envelope[]> ExecuteToEnvelopes(this NpgsqlCommand command,
            CancellationToken cancellation = default(CancellationToken))
        {
            using (var reader = await command.ExecuteReaderAsync(cancellation))
            {
                var list = new List<Envelope>();

                while (await reader.ReadAsync(cancellation))
                {
                    var bytes = await reader.GetFieldValueAsync<byte[]>(0, cancellation);
                    var envelope = Envelope.Deserialize(bytes);

                    if (reader.FieldCount == 3)
                    {
                        envelope.Status = await reader.GetFieldValueAsync<string>(1, cancellation);
                        envelope.OwnerId = await reader.GetFieldValueAsync<int>(2, cancellation);
                    }

                    list.Add(envelope);
                }

                return list.ToArray();
            }
        }

        public static List<Envelope> LoadEnvelopes(this NpgsqlCommand command)
        {
            using (var reader = command.ExecuteReader())
            {
                var list = new List<Envelope>();

                while (reader.Read())
                {
                    var bytes = reader.GetFieldValue<byte[]>(0);
                    var envelope = Envelope.Deserialize(bytes);
                    envelope.Status = reader.GetFieldValue<string>(1);
                    envelope.OwnerId = reader.GetFieldValue<int>(2);


                    if (!reader.IsDBNull(3))
                    {
                        var raw = reader.GetFieldValue<DateTime>(3);


                        envelope.ExecutionTime = raw.ToUniversalTime();
                    }

                    // Attempts will come in from the Envelope.Read
                    //envelope.Attempts = reader.GetFieldValue<int>(4);

                    list.Add(envelope);
                }

                return list;
            }
        }


        public static int RunSql(this NpgsqlConnection conn, params string[] sqls)
        {
            var sql = sqls.Join(";");
            return conn.CreateCommand().Sql(sql).ExecuteNonQuery();
        }

        public static IEnumerable<T> Fetch<T>(this NpgsqlCommand cmd, string sql, Func<DbDataReader, T> transform, params object[] parameters)
        {
            cmd.WithText(sql);
            parameters.Each(x =>
            {
                var param = cmd.AddParameter(x);
                cmd.CommandText = cmd.CommandText.UseParameter(param);
            });

            var list = new List<T>();

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(transform(reader));
                }
            }

            return list;
        }

        public static string UseParameter(this string text, NpgsqlParameter parameter)
        {
            return text.ReplaceFirst("?", ":" + parameter.ParameterName);
        }

        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }

            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }


        public static void AddParameters(this NpgsqlCommand command, object parameters)
        {
            if (parameters == null) return;

            var parameterDictionary = parameters.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(parameters, null));

            foreach (var item in parameterDictionary)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = item.Key;
                parameter.Value = item.Value ?? DBNull.Value;

                command.Parameters.Add(parameter);
            }
        }

        public static NpgsqlParameter AddParameter(this NpgsqlCommand command, object value, NpgsqlDbType? dbType = null)
        {
            var name = "arg" + command.Parameters.Count;

            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;

            if (dbType.HasValue)
            {
                parameter.NpgsqlDbType = dbType.Value;
            }

            command.Parameters.Add(parameter);

            return parameter;
        }

        public static NpgsqlParameter AddNamedParameter(this NpgsqlCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);

            return parameter;
        }

        public static NpgsqlCommand With(this NpgsqlCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
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

        public static NpgsqlCommand AsSproc(this NpgsqlCommand command)
        {
            command.CommandType = CommandType.StoredProcedure;

            return command;
        }

        public static NpgsqlCommand WithJsonParameter(this NpgsqlCommand command, string name, string json)
        {
            command.Parameters.Add(name, NpgsqlDbType.Jsonb).Value = json;

            return command;
        }

        public static NpgsqlCommand Sql(this NpgsqlCommand cmd, string sql)
        {
            cmd.CommandText = sql;
            return cmd;
        }


        public static NpgsqlCommand Returns(this NpgsqlCommand command, string name, NpgsqlDbType type)
        {
            var parameter = command.AddParameter(name);
            parameter.NpgsqlDbType = type;
            parameter.Direction = ParameterDirection.ReturnValue;
            return command;
        }

        public static NpgsqlCommand WithText(this NpgsqlCommand command, string sql)
        {
            command.CommandText = sql;
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
