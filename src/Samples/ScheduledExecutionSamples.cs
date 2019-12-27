using System;
using System.Threading.Tasks;
using Baseline.Dates;

namespace Jasper.Testing.Samples
{
    public class ScheduledExecutionSamples
    {
        // SAMPLE: ScheduleSend-In-3-Days
        public async Task schedule_send(IMessageContext context, Guid issueId)
        {
            var timeout = new WarnIfIssueIsStale
            {
                IssueId = issueId
            };

            // Process the issue timeout logic 3 days from now
            await context.ScheduleSend(timeout, 3.Days());
        }
        // ENDSAMPLE

        // SAMPLE: ScheduleSend-At-5-PM-Tomorrow
        public async Task schedule_send_at_5_tomorrow_afternoon(IMessageContext context, Guid issueId)
        {
            var timeout = new WarnIfIssueIsStale
            {
                IssueId = issueId
            };

            var time = DateTime.Today.AddDays(1).AddHours(17);


            // Process the issue timeout at 5PM tomorrow
            // Do note that Jasper quietly converts this
            // to universal time in storage
            await context.ScheduleSend(timeout, time);
        }
        // ENDSAMPLE

        // SAMPLE: ScheduleSend-Yourself
        public async Task send_at_5_tomorrow_afternoon_yourself(IMessageContext context, Guid issueId)
        {
            var timeout = new WarnIfIssueIsStale
            {
                IssueId = issueId
            };

            var time = DateTime.Today.AddDays(1).AddHours(17);


            // Process the issue timeout at 5PM tomorrow
            // Do note that Jasper quietly converts this
            // to universal time in storage
            await context.SendEnvelope(new Envelope(timeout)
            {
                ExecutionTime = time
            });

        }
        // ENDSAMPLE

        // SAMPLE: ScheduleLocally-In-3-Days
        public async Task schedule_locally(IMessageContext context, Guid issueId)
        {
            var timeout = new WarnIfIssueIsStale
            {
                IssueId = issueId
            };

            // Process the issue timeout logic 3 days from now
            // in *this* system
            await context.Schedule(timeout, 3.Days());
        }
        // ENDSAMPLE


        // SAMPLE: ScheduleLocally-At-5-PM-Tomorrow
        public async Task schedule_locally_at_5_tomorrow_afternoon(IMessageContext context, Guid issueId)
        {
            var timeout = new WarnIfIssueIsStale
            {
                IssueId = issueId
            };

            var time = DateTime.Today.AddDays(1).AddHours(17);


            // Process the issue timeout at 5PM tomorrow
            // in *this* system
            // Do note that Jasper quietly converts this
            // to universal time in storage
            await context.ScheduleSend(timeout, time);
        }

        // ENDSAMPLE
    }

    public class WarnIfIssueIsStale
    {
        public Guid IssueId { get; set; }
    }
}
