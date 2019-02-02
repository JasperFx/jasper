using System;
using Jasper.Messaging.Sagas;

namespace MessagingTests.Sagas
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
        public TState Start(TStart starting)
        {
            var state = new TState {Id = starting.Id, Name = starting.Name};

            return state;
        }

        public CompleteTwo Handle(CompleteOne one, TState state)
        {
            state.OneCompleted = true;
            return new CompleteTwo();
        }

        public void Handle(CompleteTwo message, TState state)
        {
            state.TwoCompleted = true;
        }

        public void Handle(CompleteFour message, TState state)
        {
            state.FourCompleted = true;
        }


        public void Handle(TCompleteThree three, TState state)
        {
            state.ThreeCompleted = true;
        }

        public void Handle(FinishItAll finish, TState state)
        {
            MarkCompleted();
        }
    }
}
