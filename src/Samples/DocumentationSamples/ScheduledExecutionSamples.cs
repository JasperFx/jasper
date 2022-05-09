using Baseline.Dates;
using Jasper;

namespace DocumentationSamples
{
    public class ScheduledExecutionSamples
    {
        #region sample_ScheduleSend_In_3_Days
        public async Task schedule_send(IExecutionContext context, Guid issueId)
        {
            var timeout = new WarnIfIssueIsStale
            {
                IssueId = issueId
            };

            // Process the issue timeout logic 3 days from now
            await context.ScheduleSendAsync(timeout, 3.Days());
        }
        #endregion

        #region sample_ScheduleSend_At_5_PM_Tomorrow
        public async Task schedule_send_at_5_tomorrow_afternoon(IExecutionContext context, Guid issueId)
        {
            var timeout = new WarnIfIssueIsStale
            {
                IssueId = issueId
            };

            var time = DateTime.Today.AddDays(1).AddHours(17);


            // Process the issue timeout at 5PM tomorrow
            // Do note that Jasper quietly converts this
            // to universal time in storage
            await context.ScheduleSendAsync(timeout, time);
        }
        #endregion

        #region sample_ScheduleSend_Yourself
        public async Task send_at_5_tomorrow_afternoon_yourself(IExecutionContext context, Guid issueId)
        {
            var timeout = new WarnIfIssueIsStale
            {
                IssueId = issueId
            };

            var time = DateTime.Today.AddDays(1).AddHours(17);


            // Process the issue timeout at 5PM tomorrow
            // Do note that Jasper quietly converts this
            // to universal time in storage
            await context.SendEnvelopeAsync(new Envelope(timeout)
            {
                ScheduledTime = time
            });

        }
        #endregion

        #region sample_ScheduleLocally_In_3_Days
        public async Task schedule_locally(IExecutionContext context, Guid issueId)
        {
            var timeout = new WarnIfIssueIsStale
            {
                IssueId = issueId
            };

            // Process the issue timeout logic 3 days from now
            // in *this* system
            await context.ScheduleAsync(timeout, 3.Days());
        }
        #endregion


        #region sample_ScheduleLocally_At_5_PM_Tomorrow
        public async Task schedule_locally_at_5_tomorrow_afternoon(IExecutionContext context, Guid issueId)
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
            await context.ScheduleSendAsync(timeout, time);
        }

        #endregion
    }

    public class WarnIfIssueIsStale
    {
        public Guid IssueId { get; set; }
    }
}
