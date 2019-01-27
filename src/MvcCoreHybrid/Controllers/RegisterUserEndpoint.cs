using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MvcCoreHybrid.Controllers
{
    public class RegisterUser{}

    public class UserRegistered {}

    // SAMPLE: RegisterUserHandler
    public class RegisterUserHandler
    {
        public async Task<UserRegistered> Handle(
            // The first argument is assumed to be the message type
            RegisterUser user,

            // Additional arguments are assumed to be "method injected"
            // by the application's underlying container
            UserDbContext context)
        {
            // do something to persist a new user

            // and commit the unit of work
            await context.SaveChangesAsync();


            // return a UserRegistered event explaining
            // what just happened here
            return new UserRegistered();
        }
    }
    // ENDSAMPLE


    // SAMPLE: UserController
    public class UserController : Controller
    {
        private readonly ICommandBus _bus;

        public UserController(ICommandBus bus)
        {
            _bus = bus;
        }

        [HttpPost]
        public Task<UserRegistered> Register(
            [FromBody] RegisterUser registerUser)
        {
            // Executes the RegisterUser command and
            // expect a
            return _bus.Invoke<UserRegistered>(registerUser);
        }
    }
    // ENDSAMPLE



    public static class Examples
    {
        // SAMPLE: what-else-can-the-in-memory-bus-do
        public static async Task WhatElseCanItDo(RegisterUser user, ICommandBus bus)
        {
            // Execute the command *right this instance*,
            // and because we didn't ask for the UserRegistered event,
            // Jasper enqueues *that* message for execution
            await bus.Invoke(user);


            // Enqueues the command locally for execution
            // on a background thread
            await bus.Enqueue(user);


            // Enqueues the command locally for execution,
            // but specify a pre-configured worker queue
            // name if you want to give some messages a
            // higher or lower priority
            await bus.Enqueue(user, "important");


            // Enqueue the command locally, but first persist
            // the command to durable storage for "guaranteed
            // execution"
            await bus.EnqueueDurably(user);


            // Schedule the command to be executed in 5 minutes
            await bus.Schedule(user, 5.Minutes());


            // Schedule the command to be executed at 1AM tomorrow
            await bus.Schedule(user, DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(1));
        }
        // ENDSAMPLE
    }
}
