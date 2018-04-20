using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Jasper.SqlServer.Util
{
    public class CommandBuilder : IDisposable
    {
        private readonly SqlCommand _command;


        private readonly StringBuilder _sql = new StringBuilder();

        public CommandBuilder(SqlCommand command)
        {
            _command = command;
        }

        public void Dispose()
        {
        }

        public static SqlCommand BuildCommand(Action<CommandBuilder> configure)
        {
            var cmd = new SqlCommand();
            using (var builder = new CommandBuilder(cmd))
            {
                configure(builder);

                cmd.CommandText = builder.ToString();
            }

            return cmd;
        }

        public void Apply()
        {
            _command.CommandText = _sql.ToString();
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

        public SqlParameter AddParameter(object value, SqlDbType? dbType = null)
        {
            return _command.AddParameter(value, dbType);
        }

        public SqlParameter AddNamedParameter(string name, object value)
        {
            return _command.AddNamedParameter(name, value);
        }
    }
}
