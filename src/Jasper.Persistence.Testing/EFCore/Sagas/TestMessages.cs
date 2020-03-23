using System;

namespace Jasper.Persistence.Testing.EFCore.Sagas
{
    public abstract class WorkflowState<T>
    {
        public T Id { get; set; }

        public bool OneCompleted { get; set; }
        public bool TwoCompleted { get; set; }
        public bool ThreeCompleted { get; set; }
        public bool FourCompleted { get; set; }

        public string Name { get; set; }
    }

    public class GuidWorkflowState : WorkflowState<Guid>
    {
    }

    public class IntWorkflowState : WorkflowState<int>
    {
    }

    public class LongWorkflowState : WorkflowState<long>
    {
    }

    public class StringWorkflowState : WorkflowState<string>
    {
    }

    public abstract class Start<T>
    {
        public T Id { get; set; }
        public string Name { get; set; }
    }

    public class GuidStart : Start<Guid>
    {
    }

    public class IntStart : Start<int>
    {
    }

    public class LongStart : Start<long>
    {
    }

    public class StringStart : Start<string>
    {
    }


    public abstract class CompleteThree<T>
    {
        public T SagaId { get; set; }
    }

    public class GuidCompleteThree : CompleteThree<Guid>
    {
    }

    public class IntCompleteThree : CompleteThree<int>
    {
    }

    public class LongCompleteThree : CompleteThree<long>
    {
    }

    public class StringCompleteThree : CompleteThree<string>
    {
    }


    public class CompleteOne
    {
    }

    public class CompleteTwo
    {
    }

    public class CompleteFour
    {
    }

    public class FinishItAll
    {
    }


    public class WildcardStart
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class BasicWorkflow<TState, TStart, TCompleteThree, TId> : StatefulSagaOf<TState>
        where TCompleteThree : CompleteThree<TId>
        where TStart : Start<TId>
        where TState : WorkflowState<TId>, new()

    {
        private readonly SagaDbContext _context;

        public BasicWorkflow(SagaDbContext context)
        {
            _context = context;
        }

        public TState Start(TStart starting)
        {
            var state = new TState {Id = starting.Id, Name = starting.Name};

            return state;
        }

        public CompleteTwo Handle(CompleteOne one)
        {
            State.OneCompleted = true;
            return new CompleteTwo();
        }

        public void Handle(CompleteTwo message)
        {
            State.TwoCompleted = true;
        }

        public void Handle(CompleteFour message)
        {
            State.FourCompleted = true;
        }


        public void Handle(TCompleteThree three)
        {
            State.ThreeCompleted = true;
        }

        public void Handle(FinishItAll finish)
        {
            MarkCompleted();
        }
    }
}
