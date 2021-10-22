using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Jasper.Configuration;
using Lamar;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using Microsoft.EntityFrameworkCore;
using Baseline;
using Jasper.Persistence.Sagas;
using Jasper.Runtime.Handlers;
using LamarCodeGeneration;

namespace Jasper.Persistence.EntityFrameworkCore.Codegen
{
    public class EFCorePersistenceFrameProvider : BaseSagaPersistenceFrameProvider, ITransactionFrameProvider
    {
        public override Frame DetermineStoreOrDeleteFrame(IContainer container, HandlerChain chain,
            MethodCall sagaHandler,
            Variable document, Type sagaHandlerType)
        {
            var dbType = DetermineDbContextType(chain, container);
            return new StoreOrDeleteSagaStateFrame(dbType, document, sagaHandlerType);
        }

        protected override Frame buildPersistenceFrame(IContainer container, HandlerChain chain,
            SagaStateExistence existence, ref Variable sagaId, Type sagaStateType,
            Variable existingState,
            ref Variable loadedState)
        {
            var dbContextType = DetermineDbContextType(chain, container);

            var frame = new TransactionalFrame(dbContextType);
            if (existence == SagaStateExistence.Existing)
            {
                var doc = frame.LoadDocument(sagaStateType, sagaId);
                loadedState = doc;
            }
            else
            {
                var property = findIdProperty(sagaStateType);
                sagaId = new Variable(property.PropertyType,
                    existingState.Usage + "." + property.Name);


                frame.InsertEntity(existingState);

                loadedState = existingState;
            }

            return frame;
        }

        public class EnrollDbContextInTransaction : SyncFrame
        {
            private readonly Type _dbContextType;
            private Variable _context;
            private Variable _dbContext;

            public EnrollDbContextInTransaction(Type dbContextType)
            {
                _dbContextType = dbContextType;


            }

            public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
            {
                writer.WriteComment("Enroll the DbContext & IMessagingContext in the outgoing Jasper outbox transaction");
                writer.Write(
                    $"await {typeof(JasperEnvelopeEntityFrameworkCoreExtensions).FullName}.{nameof(JasperEnvelopeEntityFrameworkCoreExtensions.EnlistInTransaction)}({_context.Usage}, {_dbContext.Usage});");

                Next?.GenerateCode(method, writer);
            }

            public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
            {
                _context = chain.FindVariable(typeof(IExecutionContext));
                yield return _context;

                _dbContext = chain.FindVariable(_dbContextType);
                yield return _dbContext;
            }
        }

        public void ApplyTransactionSupport(IChain chain, IContainer container)
        {
            var dbType = DetermineDbContextType(chain, container);

            chain.Middleware.Insert(0, new EnrollDbContextInTransaction(dbType));


            var saveChangesAsync = dbType.GetMethod(nameof(DbContext.SaveChangesAsync), new Type[]{typeof(CancellationToken)});
            var @call = new MethodCall(dbType, saveChangesAsync);
            @call.CommentText = "Added by EF Core Transaction Middleware";

            chain.Postprocessors.Add(@call);

            if (chain.ShouldFlushOutgoingMessages())
            {
                chain.Postprocessors.Add(MethodCall.For<IExecutionContext>(x => x.SendAllQueuedOutgoingMessages()));
            }
        }

        public static Type DetermineDbContextType(IChain chain, IContainer container)
        {
            var contextTypes = chain.ServiceDependencies(container).Where(x => x.CanBeCastTo<DbContext>()).ToArray();

            if (contextTypes.Length == 0)
            {
                throw new InvalidOperationException($"Cannot determine the {nameof(DbContext)} type for {chain.Description}");
            }

            if (contextTypes.Length > 1)
            {
                throw new InvalidOperationException(
                    $"Cannot determine the {nameof(DbContext)} type for {chain.Description}, multiple {nameof(DbContext)} types detected: {contextTypes.Select(x => x.Name).Join(", ")}");
            }

            return contextTypes.Single();
        }
    }
}
