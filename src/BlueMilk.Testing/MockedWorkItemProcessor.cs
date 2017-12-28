using System;

namespace Jasper.Testing.Internals
{
    public class WorkItem
    {
        public DateTime Started { get; set; }
    }

    public interface IClock
    {
        DateTime Now();
    }

    public class Clock : IClock
    {
        public DateTime Now()
        {
            return DateTime.UtcNow;
        }
    }

    public class MockedWorkItemProcessor
    {
        private readonly IClock _clock;

        public MockedWorkItemProcessor(IClock clock)
        {
            _clock = clock;
        }

        public void CheckItem(WorkItem item)
        {
            if (_clock.Now().Subtract(item.Started).Days > 5)
            {
                // yell at the developer
            }
        }
    }


    public class PushBasedWorkItemProcessor
    {
        public void CheckItem(WorkItem item)
        {
            CheckItem(item, DateTime.UtcNow);
        }

        private void CheckItem(WorkItem item, DateTime utcNow)
        {
            if (utcNow.Subtract(item.Started).Days > 5)
            {
                // yell at the developer
            }
        }
    }
}
