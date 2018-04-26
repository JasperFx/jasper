using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Marten.Tests.Setup;
using Jasper.Messaging.Tracking;
using Jasper.Testing.Messaging.Runtime;
using Marten;
using Marten.Schema;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Marten.Tests.Sample
{
    public class MessageInvocationTests : IDisposable
    {
        private readonly JasperRuntime theRuntime;

        public MessageInvocationTests()
        {
            theRuntime = JasperRuntime.For<SampleApp>();

            theRuntime.Get<IDocumentStore>().Advanced.Clean.CompletelyRemoveAll();
        }

        public void Dispose()
        {
            theRuntime?.Dispose();
        }


        [Fact]
        public async Task using_InvokeMessageAndWait()
        {
            await theRuntime.InvokeMessageAndWait(new CreateUser {Name = "Bill"});

            using (var session = theRuntime.Get<IDocumentStore>().QuerySession())
            {
                session.Load<User>("Bill").ShouldNotBeNull();
            }

            theRuntime.Get<UserNames>()
                .Names.Single().ShouldBe("Bill");
        }

        //[Fact] -- unreliable. May not actually be useful.
        public async Task using_ExecuteAndWait()
        {
            await theRuntime.ExecuteAndWait(() =>
            {
                return theRuntime.Messaging.Invoke(new CreateUser {Name = "Tom"});
            });


            using (var session = theRuntime.Get<IDocumentStore>().QuerySession())
            {
                session.Load<User>("Tom").ShouldNotBeNull();
            }

            theRuntime.Get<UserNames>()
                .Names.Single().ShouldBe("Tom");
        }

        [Fact]
        public async Task using_ExecuteAndWaitSync()
        {
            await theRuntime.ExecuteAndWait(() =>
            {
                theRuntime.Messaging.Invoke(new CreateUser {Name = "Tom"});
            });


            using (var session = theRuntime.Get<IDocumentStore>().QuerySession())
            {
                session.Load<User>("Tom").ShouldNotBeNull();
            }

            theRuntime.Get<UserNames>()
                .Names.Single().ShouldBe("Tom");
        }
    }

    // SAMPLE: AppUsingMessageTracking
    public class AppUsingMessageTracking : JasperRegistry
    {
        public AppUsingMessageTracking()
        {
            // TODO -- need to change this when GH-346 is done
            Hosting.ConfigureAppConfiguration((context, config) =>
            {
                var environment = context.HostingEnvironment.EnvironmentName;

                if (environment == "Development" || environment == "Testing")
                {
                    // Don't use this in production because it'd
                    // cause a memory leak issue
                    Include<MessageTrackingExtension>();
                }
            });


        }
    }
    // ENDSAMPLE


    public class tests_against_AppUsingMessageTracking
    {
        // SAMPLE: invoke_a_message_with_tracking
        public async Task invoke_a_message()
        {
            using (var runtime = JasperRuntime.For<AppUsingMessageTracking>())
            {
                await runtime.InvokeMessageAndWait(new Message1());

                // check the change in system state after the original
                // message and all of its cascading messages
                // finish
            }
        }
        // ENDSAMPLE

        // SAMPLE: other-message-tracking-usages
        public async Task other_usages()
        {
            using (var runtime = JasperRuntime.For<AppUsingMessageTracking>())
            {
                // Call IMessageContext.Invoke() and wait for all activity to finish
                await runtime.InvokeMessageAndWait(new Message1());

                // Configurable timeouts
                await runtime.InvokeMessageAndWait(new Message1(),
                    timeoutInMilliseconds:10000);

                // More general usage
                await runtime.ExecuteAndWait(() =>
                {
                    return runtime.Messaging.Send(new Message1());
                });

                // More general usage, but synchronously
                await runtime.ExecuteAndWait(() =>
                {
                    runtime.Messaging.Send(new Message1());
                });

                // Using an isolated message context
                await runtime.ExecuteAndWait(c => c.Send(new Message1()));

                // Assert that there were no exceptions during the processing
                // If there are, this will throw an AggregateException of
                // all encountered exceptions in the message processing
                await runtime.ExecuteAndWait(c => c.Send(new Message1()),
                    assertNoExceptions:true);
            }
        }
        // ENDSAMPLE
    }


    public class SampleApp : JasperRegistry
    {
        public SampleApp()
        {
            Settings.Alter<StoreOptions>(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
            });

            Transports.ExecuteAllMessagesLocally();
            Services.AddSingleton<UserNames>();


            Include<MessageTrackingExtension>();
        }
    }

    public class UserHandler
    {
        // SAMPLE: UserHandler-handle-CreateUser
        [MartenTransaction]
        public static UserCreated Handle(CreateUser message, IDocumentSession session)
        {
            session.Store(new User{Name = message.Name});

            return new UserCreated{UserName = message.Name};
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
        [Identity]
        public string Name;

    }
}
