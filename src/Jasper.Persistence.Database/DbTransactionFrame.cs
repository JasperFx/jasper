﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace Jasper.Persistence.Database
{
    public class DbTransactionFrame<TTransaction, TConnection> : AsyncFrame
    {
        private Variable _connection;
        private Variable _context;
        private bool _isUsingPersistence;

        public DbTransactionFrame()
        {

            Transaction = new Variable(typeof(TTransaction), this);
        }

        public bool ShouldFlushOutgoingMessages { get; set; }

        public Variable Transaction { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"await {_connection.Usage}.{nameof(DbConnection.OpenAsync)}();");
            writer.Write($"var {Transaction.Usage} = {_connection.Usage}.{nameof(DbConnection.BeginTransaction)}();");


            if (_context != null && _isUsingPersistence)
                writer.Write(
                    $"await {typeof(DbOutboxExtensions).FullName}.{nameof(DbOutboxExtensions.EnlistInTransaction)}({_context.Usage}, {Transaction.Usage});");


            Next?.GenerateCode(method, writer);
            writer.Write($"{Transaction.Usage}.{nameof(DbTransaction.Commit)}();");


            if (ShouldFlushOutgoingMessages)
            {
                writer.Write($"await {_context.Usage}.{nameof(IExecutionContext.SendAllQueuedOutgoingMessages)}();");
            }

            writer.Write($"{_connection.Usage}.{nameof(DbConnection.Close)}();");
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _isUsingPersistence = chain.IsUsingSqlServerPersistence();

            _connection = chain.FindVariable(typeof(TConnection));
            yield return _connection;


            if (ShouldFlushOutgoingMessages)
                _context = chain.FindVariable(typeof(IExecutionContext));
            else
                _context = chain.TryFindVariable(typeof(IExecutionContext), VariableSource.NotServices);

            if (_context != null) yield return _context;
        }
    }


    internal static class MethodVariablesExtensions
    {
        internal static bool IsUsingSqlServerPersistence(this IMethodVariables method)
        {
            return method.TryFindVariable(typeof(DatabaseBackedPersistenceMarker), VariableSource.NotServices) != null;
        }
    }
}
