using System;
using System.Text;
using Npgsql;
using NpgsqlTypes;

namespace Jasper.Persistence.Postgresql.Util
{
    // Imported from Marten
    internal class CommandBuilder : IDisposable
    {
        private readonly NpgsqlCommand _command;


        // TEMP -- will shift this to being pooled later
        private readonly StringBuilder _sql = new StringBuilder();

        public CommandBuilder(NpgsqlCommand command)
        {
            _command = command;
        }

        public void Dispose()
        {
        }

        public static NpgsqlCommand BuildCommand(Action<CommandBuilder> configure)
        {
            var cmd = new NpgsqlCommand();
            using (var builder = new CommandBuilder(cmd))
            {
                configure(builder);

                cmd.CommandText = builder.ToString();
            }

            return cmd;
        }

        public void Append(string text)
        {
            _sql.Append(text);
        }

        public void Append(object o)
        {
            _sql.Append(o);
        }


        public override string ToString()
        {
            return _sql.ToString();
        }

        public void Clear()
        {
            _sql.Clear();
        }


        public void AddParameters(object parameters)
        {
            _command.AddParameters(parameters);
        }

        public NpgsqlParameter AddParameter(object value, NpgsqlDbType? dbType = null)
        {
            return _command.AddParameter(value, dbType);
        }

        public NpgsqlParameter AddJsonParameter(string json)
        {
            return _command.AddParameter(json, NpgsqlDbType.Jsonb);
        }

        public NpgsqlParameter AddNamedParameter(string name, object value)
        {
            return _command.AddNamedParameter(name, value);
        }

        public void UseParameter(NpgsqlParameter parameter)
        {
            var sql = _sql.ToString();
            _sql.Clear();
            _sql.Append(sql.UseParameter(parameter));
        }

        public void Apply()
        {
            _command.CommandText = _sql.ToString();
        }

    }
}
