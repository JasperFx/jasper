using System.Collections.Generic;
using System.Data.SqlClient;
using Jasper.Messaging;
using LamarCompiler;
using LamarCompiler.Frames;
using LamarCompiler.Model;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public class SqlTransactionFrame : AsyncFrame
    {
        private Variable _connection;
        private Variable _context;
        private bool _isUsingPersistence;

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
                writer.Write(
                    $"await {typeof(SqlServerOutboxExtensions).FullName}.{nameof(SqlServerOutboxExtensions.EnlistInTransaction)}({_context.Usage}, {Transaction.Usage});");


            Next?.GenerateCode(method, writer);
            writer.Write($"{Transaction.Usage}.{nameof(SqlTransaction.Commit)}();");


            if (ShouldFlushOutgoingMessages)
                writer.Write($"await {_context.Usage}.{nameof(IMessageContext.SendAllQueuedOutgoingMessages)}();");

            writer.Write($"{_connection.Usage}.{nameof(SqlConnection.Close)}();");
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _isUsingPersistence = chain.IsUsingSqlServerPersistence();

            _connection = chain.FindVariable(typeof(SqlConnection));
            yield return _connection;


            if (ShouldFlushOutgoingMessages)
                _context = chain.FindVariable(typeof(IMessageContext));
            else
                _context = chain.TryFindVariable(typeof(IMessageContext), VariableSource.NotServices);

            if (_context != null) yield return _context;
        }
    }
}
