using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper;
using Jasper.Http;
using Jasper.Messaging;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreHosted
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()

                // This is all you have to do to use Jasper
                // with all its default behavior
                .UseJasper();
        }
    }


    public class CreateUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    public class UserCreated
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    public class User
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    public class UserHandler
    {
        public Task Handle(CreateUser user)
        {
            return Console.Out.WriteLineAsync("Hey, I got a user");
        }
    }

    public class UserController : ControllerBase
    {
        private readonly IMessageContext _messages;

        public UserController(IMessageContext messages)
        {
            _messages = messages;
        }

        [HttpPost("user")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUser user)
        {
            // Process the message NOW MISTER
            await _messages.Invoke(user);

            // OR

            // Put the incoming message in the in
            // memory worker queues and Jasper
            // will process it when it can
            await _messages.Enqueue(user);

            // OR, not sure why you'd want to do this, but say
            // you want this processed in 5 minutes
            await _messages.Schedule(user, 5.Seconds());

            return Ok();
        }
    }
}
