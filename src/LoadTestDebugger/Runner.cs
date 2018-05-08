using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Alba;
using Jasper;
using Jasper.Messaging.Runtime;
using JasperHttpTesting;
using Marten;
using Marten.Schema;
using Microsoft.Extensions.Configuration;
using Receiver;
using Sender;
using Shouldly;
using TestMessages;
using Xunit;

namespace LoadTestDebugger
{
    public class Runner : IDisposable
    {
        private JasperRuntime _sender;
        private JasperRuntime _receiver;

        public Runner()
        {
            _sender = JasperRuntime.For<SenderApp>(app =>
            {
                app.Configuration.AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"marten", "Host=localhost;Port=5433;Database=postgres;Username=postgres;password=postgres"},
                    {"receiver", "tcp://localhost:2222/durable"},
                    {"listener", "tcp://localhost:2333/durable"}
                });
            });

            //_sender.Get<IDocumentStore>().Tenancy.Default.EnsureStorageExists(typeof(Envelope));
            //_sender.Get<IDocumentStore>().Tenancy.Default.EnsureStorageExists(typeof(SentTrack));
            //_sender.Get<IDocumentStore>().Tenancy.Default.EnsureStorageExists(typeof(ReceivedTrack));

            _receiver = JasperRuntime.For<ReceiverApp>(app =>
            {
                app.Configuration.AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"marten", "Host=localhost;Port=5433;Database=postgres;Username=postgres;password=postgres"},
                    {"listener", "tcp://localhost:2222/durable"}
                });
            });

            //_receiver.Get<IDocumentStore>().Tenancy.Default.EnsureStorageExists(typeof(Envelope));
            //_receiver.Get<IDocumentStore>().Tenancy.Default.EnsureStorageExists(typeof(SentTrack));
            //_receiver.Get<IDocumentStore>().Tenancy.Default.EnsureStorageExists(typeof(ReceivedTrack));
        }

        public void Dispose()
        {
            _sender?.Dispose();
            _receiver?.Dispose();
        }





        [Fact]
        public async Task receiver_responds_to_get()
        {
            await _receiver.Scenario(_ =>
            {
                _.Get.Url("/");
                _.StatusCodeShouldBeOk();
            });
        }

        [Fact]
        public async Task receiver_responds_to_clear()
        {
            await _receiver.Scenario(_ =>
            {
                _.Post.Url("/marten/clear");
                _.StatusCodeShouldBeOk();
            });
        }

        [Fact]
        public async Task sender_responds_to_get()
        {
            await _sender.Scenario(_ =>
            {
                _.Get.Url("/");
                _.StatusCodeShouldBeOk();
            });
        }

        [Fact]
        public async Task sender_responds_to_clear()
        {
            await _sender.Scenario(_ =>
            {
                _.Post.Url("/clear");
                _.StatusCodeShouldBeOk();
            });
        }

        [Fact]
        public async Task sender_shoots_off_the_one_to_marten()
        {
            for (int i = 0; i < 10; i++)
            {
                await _sender.Scenario(_ =>
                {
                    _.Post.Url("/one");
                    _.StatusCodeShouldBeOk();
                });
            }





            var store = _sender.Get<IDocumentStore>();
            using (var session = store.QuerySession())
            {
                (await session.Query<SentTrack>().CountAsync()).ShouldBeGreaterThan(0);
            }
        }

        [Fact]
        public async Task sender_shoots_off_the_two_to_marten()
        {
            await _sender.Scenario(_ =>
            {
                _.Post.Url("/two");
                _.StatusCodeShouldBeOk();
            });

            var store = _sender.Get<IDocumentStore>();
            using (var session = store.QuerySession())
            {
                (await session.Query<SentTrack>().CountAsync()).ShouldBeGreaterThan(0);
            }
        }

        [Fact]
        public async Task sender_shoots_off_the_three_to_marten()
        {
            await _sender.Scenario(_ =>
            {
                _.Post.Url("/three");
                _.StatusCodeShouldBeOk();
            });

            var store = _sender.Get<IDocumentStore>();
            using (var session = store.QuerySession())
            {
                (await session.Query<SentTrack>().CountAsync()).ShouldBeGreaterThan(0);
            }
        }


        [Fact]
        public async Task sender_shoots_off_the_four_to_marten()
        {
            await _sender.Scenario(_ =>
            {
                _.Post.Url("/four");
                _.StatusCodeShouldBeOk();
            });

            var store = _sender.Get<IDocumentStore>();
            using (var session = store.QuerySession())
            {
                (await session.Query<SentTrack>().CountAsync()).ShouldBeGreaterThan(0);
            }
        }
    }
}
