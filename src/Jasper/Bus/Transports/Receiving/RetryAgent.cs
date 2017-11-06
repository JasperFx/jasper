namespace Jasper.Bus.Transports.Receiving
{
    // TODO -- this might be something real later
    public class RetryAgent
    {
        /*
        private void processRetry(OutgoingMessageBatch outgoing)
        {
            // TODO -- all of this is temporary
            int maximumAttempts = 3;

            foreach (var message in outgoing.Messages)
            {
                message.SentAttempts++;
            }

            var groups = outgoing
                .Messages
                .Where(x => x.SentAttempts < maximumAttempts)
                .GroupBy(x => x.SentAttempts);

            foreach (var group in groups)
            {
                var delayTime = (group.Key * group.Key).Seconds();
                var messages = group.ToArray();

                Task.Delay(delayTime, _cancellation).ContinueWith(_ =>
                {
                    if (_cancellation.IsCancellationRequested)
                    {
                        return;
                    }

                    foreach (var message in messages)
                    {
                        _sender.Enqueue(message);
                    }
                }, _cancellation);
            }

            _persistence.PersistBasedOnSentAttempts(outgoing, maximumAttempts);
        }
        */
    }
}
