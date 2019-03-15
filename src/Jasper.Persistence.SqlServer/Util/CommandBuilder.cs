using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace Jasper.Persistence.SqlServer.Util
{
    public class CommandBuilder
    {
        private readonly DbCommand _command;


        private readonly StringBuilder _sql = new StringBuilder();

        public CommandBuilder(DbCommand command)
        {
            _command = command;
        }



        public void Apply()
        {
            _command.CommandText = _sql.ToString();
        }

        public void Append(string text)
        {
            _sql.Append(text);
        }


        public override string ToString()
        {
            return _sql.ToString();
        }

        public void Clear()
        {
            _sql.Clear();
        }


        public DbParameter AddParameter(Guid value)
        {
            return _command.AddParameter(value, DbType.Guid);
        }

        public DbParameter AddParameter(int value)
        {
            return _command.AddParameter(value, DbType.Int32);
        }

        public DbParameter AddParameter(string value)
        {
            return _command.AddParameter(value, DbType.String);
        }

        public DbParameter AddParameter(byte[] value)
        {
            return _command.AddParameter(value, DbType.Binary);
        }

        public DbParameter AddParameter(DateTimeOffset value)
        {
            return _command.AddParameter(value, DbType.DateTimeOffset);
        }

        public DbParameter AddParameter(DateTimeOffset? value)
        {
            return _command.AddParameter(value, DbType.DateTimeOffset);
        }

        public DbParameter AddNamedParameter(string name, object value)
        {
            return _command.AddNamedParameter(name, value);
        }
    }
}
