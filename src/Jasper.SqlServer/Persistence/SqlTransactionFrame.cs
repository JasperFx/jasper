using System.Collections.Generic;
using System.Data.SqlClient;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;

namespace Jasper.SqlServer.Persistence
{
    public class SqlTransactionFrame : AsyncFrame
    {
        private Variable _connection;

        public SqlTransactionFrame()
        {
            Transaction = new Variable(typeof(SqlTransaction), this);
        }

        public Variable Transaction { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"await {_connection.Usage}.{nameof(SqlConnection.OpenAsync)}();");
            writer.Write($"var {Transaction.Usage} = {_connection.Usage}.{nameof(SqlConnection.BeginTransaction)}();");

            Next?.GenerateCode(method, writer);
            writer.Write($"{Transaction.Usage}.{nameof(SqlTransaction.Commit)}();");
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _connection = chain.FindVariable(typeof(SqlConnection));
            yield return _connection;
        }
    }
}
