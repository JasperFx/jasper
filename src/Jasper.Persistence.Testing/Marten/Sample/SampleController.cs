using System.Threading.Tasks;
using Jasper.Persistence.Marten;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace Jasper.Persistence.Testing.Marten.Sample
{
    public class SampleController : ControllerBase
    {
        #region sample_using_outbox_with_marten_in_mvc_action
        public async Task<IActionResult> PostCreateUser(
            [FromBody] CreateUser user,
            [FromServices] IExecutionContext context,
            [FromServices] IDocumentSession session)
        {
            await context.EnlistInTransactionAsync(session);

            session.Store(new User {Name = user.Name});

            var @event = new UserCreated {UserName = user.Name};

            await context.PublishAsync(@event);

            await session.SaveChangesAsync();

            return Ok();
        }

        #endregion



    }


}
