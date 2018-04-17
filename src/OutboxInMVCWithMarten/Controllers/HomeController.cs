using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Marten;
using Jasper.Messaging;
using Marten;
using Microsoft.AspNetCore.Mvc;
using TestMessages;

namespace OutboxInMVCWithMarten.Controllers
{
    public class CreateItemCommand{}
    public class Item{
        public Guid Id { get; set; }
    }
    public class ItemCreated{
        public Guid Id { get; set; }
    }

    public class ItemHandler
    {
        [MartenTransaction]
        public static ItemCreated Handle(
            CreateItemCommand command,
            IDocumentSession session)
        {
            var item = createItem(command);

            session.Store(item);
            return new ItemCreated{Id = item.Id};
        }

        private static Item createItem(CreateItemCommand command)
        {
            throw new NotImplementedException();
        }
    }


    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateItem(
            [FromBody] CreateItemCommand command,
            [FromServices] IDocumentStore martenStore,
            [FromServices] IMessageContext context)
        {
            var item = createItem(command);


            using (var session = martenStore.LightweightSession())
            {
                // Directs the message context to hold onto
                // outgoing messages, and persist them
                // as part of the given Marten document
                // session when it is committed
                await context.EnlistInTransaction(session);

                var outgoing = new ItemCreated{Id = item.Id};
                await context.Send(outgoing);

                session.Store(item);

                await session.SaveChangesAsync();
            }

            return Ok();
        }

        private Item createItem(CreateItemCommand command)
        {
            throw new NotImplementedException();
        }


        // SAMPLE: using-outbox-with-marten-in-mvc-action
        [HttpPost]
        public async Task<IActionResult> CreateUser(
            string userId,
            [FromServices] IDocumentStore martenStore,
            [FromServices] IMessageContext context)
        {
            // The Marten IDocumentSession represents the unit of work
            using (var session = martenStore.OpenSession())
            {
                // This directs the current message context
                // to persist outgoing messages with this
                // Marten session.
                await context.EnlistInTransaction(session);

                var theUser = new User { Id = userId };
                session.Store(theUser);

                await context.Send(new NewUser {UserId = userId});

                // The outgoing messages will be persisted
                // and sent to the outgoing transports
                // as a result of the transaction succeeding here
                await session.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
        }
        // ENDSAMPLE








        [HttpPost]
        public async Task<IActionResult> DeleteUser(
            string userId,
            [FromServices] IDocumentStore martenStore,
            [FromServices] IMessageContext bus)
        {
            // the bus can use a document session no matter how it has been created
            using (var session = martenStore.DirtyTrackedSession(IsolationLevel.Serializable))
            {
                await bus.EnlistInTransaction(session);

                var existing = session.Load<User>(userId);
                if (existing != null && !existing.IsDeleted)
                {
                    existing.IsDeleted = true;
                    await bus.Publish(new UserDeleted { UserId = userId });
                    await session.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }

    public class User
    {
        public string Id { get; set; }
        public bool IsDeleted { get; set; }
    }
}
