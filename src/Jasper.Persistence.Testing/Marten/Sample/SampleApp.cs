using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntegrationTests;
using Jasper.Attributes;
using Jasper.Persistence.Marten;
using Jasper.Tracking;
using Marten;
using Marten.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Persistence.Testing.Marten.Sample
{
    // SAMPLE: MartenUsingApp
    public class MartenUsingApp : JasperOptions
    {
        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            // This registers the message persistence as well as
            // configuring Marten inside your application
            Extensions.UseMarten(config.GetConnectionString("database"));
        }
    }
    // ENDSAMPLE


    public class MessageInvocationTests : PostgresqlContext, IDisposable
    {
        public MessageInvocationTests()
        {
            theHost = JasperHost.For<SampleApp>();

            theHost.Get<IDocumentStore>().Advanced.Clean.CompletelyRemoveAll();
        }

        public void Dispose()
        {
            theHost?.Dispose();
        }

        private readonly IHost theHost;


        [Fact]
        public async Task using_ExecuteAndWaitSync()
        {
            await theHost.ExecuteAndWait(x => x.Invoke(new CreateUser {Name = "Tom"}));


            using (var session = theHost.Get<IDocumentStore>().QuerySession())
            {
                session.Load<User>("Tom").ShouldNotBeNull();
            }

            theHost.Get<UserNames>()
                .Names.Single().ShouldBe("Tom");
        }


        [Fact]
        public async Task using_InvokeMessageAndWait()
        {
            await theHost.ExecuteAndWait(x => x.Invoke(new CreateUser {Name = "Bill"}));

            using (var session = theHost.Get<IDocumentStore>().QuerySession())
            {
                session.Load<User>("Bill").ShouldNotBeNull();
            }

            theHost.Get<UserNames>()
                .Names.Single().ShouldBe("Bill");
        }
    }

    // SAMPLE: AppUsingMessageTracking
    public class AppUsingMessageTracking : JasperOptions
    {
        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            if (hosting.IsDevelopment() || hosting.IsEnvironment("Testing"))
            {
                // This is necessary to add the message tracking
                // to your Jasper application
                Extensions.UseMessageTrackingTestingSupport();
            }
        }
    }
    // ENDSAMPLE


    public class tests_against_AppUsingMessageTracking
    {
        // SAMPLE: invoke_a_message_with_tracking
        public async Task invoke_a_message()
        {
            using (var host = JasperHost.For<AppUsingMessageTracking>())
            {
                await host.ExecuteAndWait(x => x.Invoke(new Message1()));

                // check the change in system state after the original
                // message and all of its cascading messages
                // finish
            }
        }
        // ENDSAMPLE

        // SAMPLE: other-message-tracking-usages
        public async Task other_usages()
        {
            using (var runtime = JasperHost.For<AppUsingMessageTracking>())
            {
                // Call IMessageContext.Invoke() and wait for all activity to finish
                await runtime.InvokeMessageAndWait(new Message1());

                // Configurable timeouts
                await runtime.InvokeMessageAndWait(new Message1(),
                    10000);

                // More general usage to send a single message and wait
                // for all activity to complete
                await runtime.ExecuteAndWait(() => runtime.Send(new Message1()));


                // Using an isolated message context
                await runtime.ExecuteAndWait(c => c.Send(new Message1()));

                // Assert that there were no exceptions during the processing
                // If there are, this will throw an AggregateException of
                // all encountered exceptions in the message processing
                var session = await runtime.ExecuteAndWait(c => c.Send(new Message1()));
            }
        }

        // ENDSAMPLE
    }


    public class SampleApp : JasperOptions
    {
        public SampleApp()
        {

            Endpoints.PublishAllMessages().Locally();

            Services.AddSingleton<UserNames>();

            Extensions.UseMarten(Servers.PostgresConnectionString);
            Extensions.Include<MessageTrackingExtension>();
        }
    }

    public class UserHandler
    {
        // SAMPLE: UserHandler-handle-CreateUser
        [Transactional]
        public static UserCreated Handle(CreateUser message, IDocumentSession session)
        {
            session.Store(new User {Name = message.Name});

            return new UserCreated {UserName = message.Name};
        }
        // ENDSAMPLE

        public static void Handle(UserCreated message, UserNames names)
        {
            names.Names.Add(message.UserName);
        }
    }

    public class CreateUser
    {
        public string Name;
    }

    public class UserCreated
    {
        public string UserName;
    }

    public class UserNames
    {
        public readonly IList<string> Names = new List<string>();
    }

    public class User
    {
        [Identity] public string Name;
    }
}
