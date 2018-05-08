using System.Collections.Generic;
using System.Data.SqlClient;
using Jasper.Messaging;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;

namespace Jasper.SqlServer.Persistence
{
    public class SqlTransactionFrame : AsyncFrame
    {
        private Variable _connection;
        private bool _isUsingPersistence;
        private Variable _context;

        public SqlTransactionFrame()
        {
            Transaction = new Variable(typeof(SqlTransaction), this);
        }

        public bool ShouldFlushOutgoingMessages { get; set; }

        public Variable Transaction { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"await {_connection.Usage}.{nameof(SqlConnection.OpenAsync)}();");
            writer.Write($"var {Transaction.Usage} = {_connection.Usage}.{nameof(SqlConnection.BeginTransaction)}();");


            if (_context != null && _isUsingPersistence)
            {
                writer.Write($"await {typeof(SqlServerOutboxExtensions).FullName}.{nameof(SqlServerOutboxExtensions.EnlistInTransaction)}({_context.Usage}, {Transaction.Usage});");
            }


            Next?.GenerateCode(method, writer);
            writer.Write($"{Transaction.Usage}.{nameof(SqlTransaction.Commit)}();");


            if (ShouldFlushOutgoingMessages)
            {
                writer.Write($"await {_context.Usage}.{nameof(IMessageContext.SendAllQueuedOutgoingMessages)}();");
            }

            writer.Write($"{_connection.Usage}.{nameof(SqlConnection.Close)}();");
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _isUsingPersistence = chain.IsUsingSqlServerPersistence();

            _connection = chain.FindVariable(typeof(SqlConnection));
            yield return _connection;


            if (ShouldFlushOutgoingMessages)
            {
                _context = chain.FindVariable(typeof(IMessageContext));
            }
            else
            {
                // Inside of messaging. Not sure how this is gonna work for HTTP yet
                _context = chain.TryFindVariable(typeof(IMessageContext), VariableSource.NotServices);
            }



            if (_context != null) yield return _context;
        }
    }
}
