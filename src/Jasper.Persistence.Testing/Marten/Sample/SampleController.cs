using System.Threading.Tasks;
using Jasper.Persistence.Marten;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace Jasper.Persistence.Testing.Marten.Sample
{
    public class SampleController : ControllerBase
    {
        // SAMPLE: using-outbox-with-marten-in-mvc-action
        public async Task<IActionResult> PostCreateUser(
            [FromBody] CreateUser user,
            [FromServices] IExecutionContext context,
            [FromServices] IDocumentSession session)
        {
            await context.EnlistInTransaction(session);

            session.Store(new User {Name = user.Name});

            var @event = new UserCreated {UserName = user.Name};

            await context.Publish(@event);

            await session.SaveChangesAsync();

            return Ok();
        }

        // ENDSAMPLE



    }


}
