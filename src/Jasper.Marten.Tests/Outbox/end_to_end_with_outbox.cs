using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Marten.Tests.Setup;
using Jasper.Messaging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Testing;
using Marten;
using Shouldly;
using Xunit;

namespace Jasper.Marten.Tests.Outbox
{
    ///<summary>
    /// In this example, the OrdersApp would be running in an
    /// ASP.Net Core app which receives an HTTP request to create an order.
    ///
    /// While processing the HTTP request, it creates the order in its
    /// database and sends a ProcessOrder command message.
    ///
    /// A WarehouseApp consumes that command, and its handler
    /// creates a picklist in its own database and publishes an
    /// ItemOutOfStock event which is delivered back to the OrdersApp.
    /// </summary>
    public class end_to_end_with_outbox : IDisposable
    {
        private JasperRuntime theSender;
        private JasperRuntime theHandler;

        private static int portCounter = 2222;

        public end_to_end_with_outbox()
        {
            using (var store = DocumentStore.For(ConnectionSource.ConnectionString))
            {
                store.Advanced.Clean.CompletelyRemoveAll();
            }

            var senderPort = portCounter;

            theSender = JasperRuntime.For(new OrdersApp(senderPort));
            // can switch between a few different implementations of the warehouse handler.
            theHandler = JasperRuntime.For(new WarehouseApp<WarehouseHandler2>(senderPort));

            portCounter += 2;
        }

        public void Dispose()
        {
            theSender?.Dispose();
            theHandler?.Dispose();
        }

        [Fact]
        public void code_generation_includes_the_call_to_enlist_the_transaction()
        {
            var code = theSender.Get<HandlerGraph>().ChainFor<ItemOutOfStock>().SourceCode;

            code.ShouldContain("await Jasper.Marten.MessageContextExtensions.EnlistInTransaction(context, documentSession);");
        }

        [Fact]
        public async Task send_and_handle_end_to_end()
        {
            // when this code appears in an MVC controller, both the IDocumentSession and the IServiceBus could be injected dependencies.
            using (var session = theSender.Get<IDocumentStore>().OpenSession())
            {
                var bus = theSender.Get<IMessageContext>();

                await bus.EnlistInTransaction(session);

                var order = new Order {Id = Guid.NewGuid(), ItemName = "Hat"};
                session.Store(order);

                var commandMessage = new ProcessOrder {Id = order.Id, ItemName = order.ItemName};
                await bus.Send(commandMessage);

                //TODO: assert that at this point, there should not have been anything handed to the sending agent.

                // SaveChangesAsync will: save the document session including the outgoing envelopes,
                // then trigger the sending agent to start delivering those envelopes.
                await session.SaveChangesAsync();
                //TODO: assert that at this point, the sending agent should have been told to deliver the message
            }

            TestSynch.HandledProcessOrderCommand.WaitOne(5.Seconds()).ShouldBe(true, "Waited too long for ProcessOrder event to be handled");
            //TODO: at this point, the warehouse app's sending agent should _not_ have been told to deliver the message
            TestSynch.WarehouseHandlerShouldContinueEvent.Set();

            TestSynch.ProcessedItemOutOfStockEvent.WaitOne(5.Seconds()).ShouldBe(true, "Waited too long for ItemOutOfStock event to be handled");
        }
    }

    public static class TestSynch
    {
        public static AutoResetEvent HandledProcessOrderCommand = new AutoResetEvent(false);
        public static AutoResetEvent WarehouseHandlerShouldContinueEvent = new AutoResetEvent(false);
        public static AutoResetEvent ProcessedItemOutOfStockEvent = new AutoResetEvent(false);
    }







    public class OrdersApp : JasperRegistry
    {
        public OrdersApp(int senderPort)
        {
            var receiverPort = senderPort + 1;
            Include<MartenBackedPersistence>();

            // Whether or not our event is destined for a durable queue, it will be stored durably in the outbox because of the usage of an outbox when sending it.
            Publish.Message<ProcessOrder>().To($"tcp://localhost:{receiverPort}/durable");

            Transports.DurableListenerAt(senderPort);

            Settings.Alter<StoreOptions>(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
                _.DatabaseSchemaName = "orders";
            });

            Handlers.DisableConventionalDiscovery();
            Handlers.IncludeType<OrderStatusHandler>();
        }
    }

    public class OrderStatusHandler
    {
        [MartenTransaction]
        public void Handle(ItemOutOfStock outOfStockNotification, IDocumentSession session)
        {
            var order = session.Load<Order>(outOfStockNotification.OrderId);
            order.Status = "Backordered";
            TestSynch.ProcessedItemOutOfStockEvent.Set();
        }
    }

    public class Order
    {
        public Guid Id { get; set; }

        public string ItemName { get; set; }

        public string Status { get; set; }
    }



    public class WarehouseApp<THandler> : JasperRegistry
    {
        public WarehouseApp(int senderPort)
        {
            var receiverPort = senderPort + 1;

            Include<MartenBackedPersistence>();

            Settings.Alter<StoreOptions>(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
            });

            //Note: whether or not our event is destined for a durable queue, it will be stored durably in the outbox because of the implementation of the handlers.
            Publish.Message<ItemOutOfStock>().To($"tcp://localhost:{senderPort}/durable");

            Transports.DurableListenerAt(receiverPort);

            Settings.Alter<StoreOptions>(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
                _.DatabaseSchemaName = "warehouse";
            });

            Handlers.DisableConventionalDiscovery();
            Handlers.IncludeType<THandler>();
        }
    }

    /* Two different alternatives for handler implementations that make use of the outbox */

    /// <summary>
    /// By using MartenTransaction and cascading messages, I expect the cascading messages to be stored
    /// to the outgoing table in the same document session as the one supplied to the handler.
    /// </summary>
    public class WarehouseHandler1
    {
        [MartenTransaction]
        public object Handle(ProcessOrder processOrder, IDocumentSession session)
        {
            session.Store(new PickList { Id = processOrder.Id});
            var outOfStockEvent = new ItemOutOfStock {OrderId = processOrder.Id, ItemName = processOrder.ItemName};

            // wait here for the test to inspect state.
            TestSynch.HandledProcessOrderCommand.Set();
            TestSynch.WarehouseHandlerShouldContinueEvent.WaitOne(5.Seconds());

            return outOfStockEvent;
        }
    }

    /// <summary>
    /// By taking a dependency on the document store and enlisting the bus to it, all messages sent through that bus
    /// will be tracked in that session.
    /// </summary>
    public class WarehouseHandler2
    {
        public async Task Handle(ProcessOrder processOrder, IDocumentStore martenStore, IMessageContext bus)
        {
            using (var session = martenStore.OpenSession())
            {
                await bus.EnlistInTransaction(session);

                session.Store(new PickList {Id = processOrder.Id});
                var outOfStockEvent = new ItemOutOfStock { OrderId = processOrder.Id, ItemName = processOrder.ItemName };

                await bus.Publish(outOfStockEvent);

                // wait here for the test to inspect state.
                TestSynch.HandledProcessOrderCommand.Set();
                TestSynch.WarehouseHandlerShouldContinueEvent.WaitOne(5.Seconds());

                await session.SaveChangesAsync();
            }
        }
    }


    public class PickList
    {
        public Guid Id { get; set; }
    }

    public class ProcessOrder
    {
        public Guid Id;
        public string ItemName;
    }

    public class ItemOutOfStock
    {
        public Guid OrderId { get; set; }
        public string ItemName { get; set; }
    }
}
