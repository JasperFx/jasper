using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.EnvironmentChecks;
using Jasper.Messaging.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.EnvironmentChecks
{
    public class EnvironmentChecksProcessing
    {
        [Fact]
        public async Task do_not_fail_if_advanced_says_not_to_blow_up()
        {
            var runtime = await JasperRuntime.ForAsync(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Services.EnvironmentCheck<NegativeCheck>();
                _.Advanced.ThrowOnValidationErrors = false;
            });

            await runtime.Shutdown();
        }


        [Fact]
        public async Task fail_on_startup_with_negative_check()
        {
            var aggregate = await Exception<AggregateException>.ShouldBeThrownByAsync(async () =>
            {
                var runtime = await JasperRuntime.ForAsync(_ =>
                {
                    _.Handlers.DisableConventionalDiscovery();

                    _.Services.EnvironmentCheck<NegativeCheck>();
                });
            });

            aggregate.InnerExceptions.Single().Message
                .ShouldContain("Kaboom!");
        }

        [Fact]
        public async Task fail_with_lambda_check()
        {
            var aggregate = await Exception<AggregateException>.ShouldBeThrownByAsync(async () =>
            {
                var runtime = await JasperRuntime.ForAsync(_ =>
                {
                    _.Handlers.DisableConventionalDiscovery();

                    _.Services.EnvironmentCheck("Bazinga!", () => throw new Exception("Bang"));
                });
            });

            aggregate.InnerExceptions.Single().Message
                .ShouldContain("Bang");
        }

        [Fact]
        public async Task fail_with_lambda_check_with_service()
        {
            var aggregate = await Exception<AggregateException>.ShouldBeThrownByAsync(async () =>
            {
                await JasperRuntime.ForAsync(_ =>
                {
                    _.Handlers.DisableConventionalDiscovery();

                    _.Services.EnvironmentCheck<Thing>("Bazinga!", t => t.ThrowUp());
                });
            });
        }

        [Fact]
        public async Task finds_checks_that_were_not_registered_as_environment_check()
        {
            var aggregate = await Exception<AggregateException>.ShouldBeThrownByAsync(async () =>
            {
                await JasperRuntime.ForAsync(_ =>
                {
                    _.Handlers.DisableConventionalDiscovery();
                    _.Services.AddTransient<ISomeService, BadService>();
                });
            });

            aggregate.InnerExceptions.Single().Message.ShouldContain("I'm bad!");
        }

        [Fact]
        public async Task succeed_with_lambda_check()
        {
            await JasperRuntime.ForAsync(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();

                _.Services.EnvironmentCheck("Bazinga!", () => { });
            });
        }

        [Fact]
        public async Task succeed_with_lambda_check_using_service()
        {
            var runtime = await JasperRuntime.ForAsync(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();

                _.Services.EnvironmentCheck<Thing>("Bazinga!", t => t.AllGood());
            });
        }

        [Fact]
        public async Task timeout_on_task()
        {
            var aggregate = await Exception<AggregateException>.ShouldBeThrownByAsync(async () =>
            {
                var runtime = await JasperRuntime.ForAsync(_ =>
                {
                    _.Handlers.DisableConventionalDiscovery();

                    _.Services.EnvironmentCheck<Thing>("Bazinga!", t => t.TooLong(), 50.Milliseconds());
                });
            });
        }
    }

    public class Thing
    {
        public void ThrowUp()
        {
            throw new Exception("I threw up");
        }

        public void AllGood()
        {
        }

        public Task TooLong()
        {
            return Task.Delay(3.Seconds());
        }
    }

    public interface ISomeService
    {
    }

    public class SomeService1 : ISomeService, IEnvironmentCheck
    {
        public void Assert(JasperRuntime runtime)
        {
            // all good
        }

        public string Description { get; } = "SomeService1";
    }

    public class BadService : ISomeService, IEnvironmentCheck
    {
        public void Assert(JasperRuntime runtime)
        {
            throw new Exception("I'm bad!");
        }

        public string Description { get; } = "BadService";
    }

    public class PositiveCheck : IEnvironmentCheck
    {
        public void Assert(JasperRuntime runtime)
        {
            // all good
        }

        public string Description { get; } = "PositiveCheck";
    }

    public class NegativeCheck : IEnvironmentCheck
    {
        public void Assert(JasperRuntime runtime)
        {
            throw new Exception("Kaboom!");
        }

        public string Description { get; } = "NegativeCheck";
    }

    public class StubEnvironmentRecorder : IEnvironmentRecorder
    {
        public readonly IDictionary<string, Exception> Failures = new Dictionary<string, Exception>();
        public readonly IList<string> Successes = new List<string>();

        public bool AssertAllWasCalled { get; set; }

        public void Success(string description)
        {
            Successes.Add(description);
        }

        public void Failure(string description, Exception exception)
        {
            Failures.Add(description, exception);
        }

        public void AssertAllSuccessful()
        {
            AssertAllWasCalled = true;
        }
    }

    // SAMPLE: registering-environment-checks
    public class AppWithEnvironmentChecks : JasperRegistry
    {
        public AppWithEnvironmentChecks()
        {
            // Register an IEnvironmentCheck object
            Services.EnvironmentCheck(new FileExistsCheck("settings.json"));

            // or declaratively say a file should exist (this is just syntactic sugar)
            Services.CheckFileExists("settings.json");

            // or do it manually w/ a lambda
            Services.EnvironmentCheck("settings.json can be found", () =>
            {
                if (!File.Exists("settings.json")) throw new Exception("File cannot be found");
            });

            // Or register a check type
            Services.EnvironmentCheck<CustomCheck>();

            // The concrete Store class exposes IEnvironmentCheck
            Services.AddTransient<IStore, Store>();
        }
    }

    public interface IStore
    {
    }

    // Jasper will still use this as an environment check
    public class Store : IStore, IEnvironmentCheck
    {
        public void Assert(JasperRuntime runtime)
        {
            // do the assertion of valid state
        }

        public string Description { get; } = "Fake Store Environment Check";



    }
    // ENDSAMPLE


    public class CustomCheck : IEnvironmentCheck
    {
        public void Assert(JasperRuntime runtime)
        {
            // do something here
        }

        public string Description { get; } = "Some Description";
    }
}
