using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Marten;
using Marten;
using Microsoft.AspNetCore.Mvc;
using TestMessages;

namespace OutboxInMVCWithMarten.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        /*

        [HttpPost]
        public async Task<IActionResult> CreateUser(
            string userId,
            [FromServices] IDocumentStore martenStore,
            [FromServices] IServiceBus theBus)
        {
            // The Marten IDocumentSession represents the unit of work
            using (var session = martenStore.OpenSession())
            {
                var theUser = new User {Id = userId};
                session.Store(theUser);

                await theBus.Send(new UserCreated {UserId = userId});

                await session.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(
            string userId,
            [FromServices] IDocumentStore martenStore,
            [FromServices] IServiceBus theBus)
        {
            using (var session = martenStore.OpenSession())
            {
                var user = await session.LoadAsync<User>(userId);
                user.IsDeleted = true;

                await session.SaveChangesAsync();

                await theBus.Send(new UserDeleted {UserId = userId});

                return RedirectToAction(nameof(Index));
            }
        }
        */

        /* With outbox */
        [HttpPost]
        public async Task<IActionResult> CreateUser(
            string userId,
            [FromServices] IDocumentStore martenStore,
            [FromServices] IServiceBus bus)
        {
            // The Marten IDocumentSession represents the unit of work
            using (var session = martenStore.OpenSession())
            {
                bus.EnlistInTransaction(session);

                var theUser = new User { Id = userId };
                session.Store(theUser);

                await bus.Send(new NewUser {UserId = userId});

                await session.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(
            string userId,
            [FromServices] IDocumentStore martenStore,
            [FromServices] IServiceBus bus)
        {
            // the bus can use a document session no matter how it has been created
            using (var session = martenStore.DirtyTrackedSession(IsolationLevel.Serializable))
            {
                bus.EnlistInTransaction(session);

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
