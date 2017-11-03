using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Transports
{
    public interface IMessageCallback
    {
        // Remove the message from the queue. Think this is going to be renamed
        // to something like "MarkComplete()"
        Task MarkSuccessful();

        // Had more meaning in fubu. Only usage is if the move to the delayed job queue fails.
        [Obsolete("Remove as part of GH-190")]
        Task MarkFailed(Exception ex);

        [Obsolete("Will be eliminated with GH-37")]
        Task MoveToErrors(ErrorReport report);


        Task Requeue(Envelope envelope);
    }
}
