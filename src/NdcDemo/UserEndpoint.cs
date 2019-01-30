using System.Threading.Tasks;
using Jasper.Persistence;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace NdcDemo
{
    public class User
    {
        public string Id { get; set; }
    }

    public class CreateUser
    {
        public string UserId { get; set; }
    }



    public class UserEndpoint
    {
        [Transactional]
        public static int post_user_create(CreateUser command, IDocumentSession session, ILogger<User> logger)
        {
            var user = new User
            {
                Id = command.UserId
            };


            session.Store(user);

            logger.LogInformation($"Created new user '{user.Id}'");

            return 201;
        }

        public static Task<User> get_user_id(string id, IQuerySession session)
        {
            return session.LoadAsync<User>(id);
        }
    }






}
