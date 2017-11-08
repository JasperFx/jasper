using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Configuration;
using Jasper.Marten.Tests.Setup;
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
    /// database and publishes an OrderPlaced event.
    /// 
    /// A WarehouseApp has subscribed to that event, and its handler
    /// creates a picklist in its own database and publishes an 
    /// ItemOutOfStock event which is delivered back to the OrdersApp.
    /// </summary>
    public class end_to_end_with_outbox : IDisposable
    {
        private JasperRuntime theSender;
        private JasperRuntime theHandler;

        public end_to_end_with_outbox()
        {
            using (var store = DocumentStore.For(ConnectionSource.ConnectionString))
            {
                store.Advanced.Clean.CompletelyRemoveAll();
            }

            theSender = JasperRuntime.For(new OrdersApp());
            // can switch between a few different implementations of the warehouse handler.
            theHandler = JasperRuntime.For(new WarehouseApp<WarehouseHandler1>());
        }

        public void Dispose()
        {
            theSender?.Dispose();
            theHandler?.Dispose();
        }

        [Fact]
        public async Task send_and_handle_end_to_end()
        {
            /* Alternative 1 (Preferred) */
            // when this code appears in an MVC controller, the outbox could be injected as a scoped dependency.
            using (var outbox = theSender.Get<MartenOutbox>())
            {
                var order = new Order {Id = Guid.NewGuid(), ItemName = "Hat"};
                outbox.DocumentSession.Store(order);

                var eventMessage = new OrderPlaced {Id = order.Id, ItemName = order.ItemName};
                outbox.Send(eventMessage);

                //TODO: assert that at this point, the sending agent should _not_ have been told to deliver the message

                // Complete will: save the document session including the envelopes in the outbox,
                // then trigger the sending agent to start delivering those envelopes.
                await outbox.Complete();
                //TODO: assert that at this point, the sending agent should have been told to deliver the message
            }

            TestSynch.ProcessedOrderPlacedEvent.WaitOne(5.Seconds()).ShouldBe(true, "Waited too long for OrderPlaced event to be handled");
            //TODO: at this point, the warehouse app's sending agent should _not_ have been told to deliver the message
            TestSynch.WarehouseHandlerShouldContinueEvent.Set();

            TestSynch.ProcessedItemOutOfStockEvent.WaitOne(5.Seconds()).ShouldBe(true, "Waited too long for ItemOutOfStock event to be handled");




            /* Alternative 2 */
            // when this code appears in an MVC controller, both the session and the outbox could be injected as scoped dependencies.
            using (var session = theSender.Get<IDocumentStore>().OpenSession())
            {
                using (var outboxBus = new MartenOutboxBus(session, theSender.Get<SessionCommitListener>()))
                {
                    var order = new Order {Id = Guid.NewGuid(), ItemName = "Hat"};
                    session.Store(order);

                    var commandMessage = new OrderPlaced {Id = order.Id, ItemName = order.ItemName};
                    outboxBus.Send(commandMessage);

                    //TODO: assert that at this point, there should not have been anything handed to the sending agent.

                    // Complete will: save the document session including the envelopes in the outbox,
                    // then trigger the sending agent to start delivering those envelopes.
                    await session.SaveChangesAsync();
                    //TODO: assert that at this point, the sending agent should have been told to deliver the message
                }
            }

            TestSynch.ProcessedOrderPlacedEvent.WaitOne(5.Seconds()).ShouldBe(true, "Waited too long for OrderPlaced event to be handled");
            //TODO: at this point, the warehouse app's sending agent should _not_ have been told to deliver the message
            TestSynch.WarehouseHandlerShouldContinueEvent.Set();

            TestSynch.ProcessedItemOutOfStockEvent.WaitOne(5.Seconds()).ShouldBe(true, "Waited too long for ItemOutOfStock event to be handled");
        }
    }

    public static class TestSynch
    {
        public static AutoResetEvent ProcessedOrderPlacedEvent = new AutoResetEvent(false);
        public static AutoResetEvent WarehouseHandlerShouldContinueEvent = new AutoResetEvent(false);
        public static AutoResetEvent ProcessedItemOutOfStockEvent = new AutoResetEvent(false);
    }


    /// <summary>
    /// Alternative 1
    /// </summary>
    /// <remarks>
    /// This represents a unit of work which combines operations on the DocumentSession with message sends over the bus. 
    /// It "owns" the document session. Users shouldn't call DocumentSession.SaveChanges directly. They should call Complete() instead.
    /// </remarks>
    public class MartenOutbox : IMartenOutbox, IDisposable
    {
        private IDocumentSession _wrappedSession;
        private readonly Func<IDocumentSession> _sessionFactory;
        private readonly List<Envelope> _messagesToSend = new List<Envelope>();

        public MartenOutbox(IDocumentStore store)
        {
            _sessionFactory = () => store.OpenSession();
        }

        public void Dispose()
        {
            _wrappedSession?.Dispose();
        }

        public IDocumentSession DocumentSession
        {
            get
            {
                lock (_sessionFactory)
                {
                    if (_wrappedSession == null)
                    {
                        _wrappedSession = _sessionFactory.Invoke();
                    }
                }
                return _wrappedSession;
            }
        }

        public void Send<T>(T message)
        {
            //TODO: use the message router to get envelopes that are ready for delivery, then hold on to them in the _messagesToSend list.
            throw new NotImplementedException();
        }

        public async Task Complete()
        {
            // store all the outgoing messages in the document session
            if (_messagesToSend.Any())
            {
                var session = DocumentSession;
                foreach (var envelope in _messagesToSend)
                {
                    //Question: should we configure Marten to store the outgoing envelopes in a separate collection from the incoming (including delayed) ones?
                    // or is it enough to use an index over the destination and/or execution time headers?
                    session.Store(envelope);
                }
            }
            if(_wrappedSession != null)
            {
                await _wrappedSession.SaveChangesAsync();
                //TODO: notify the sending agent that the sent envelopes are ready to be delivered.
            }
        }
    }

    public interface IMartenOutbox
    {
        IDocumentSession DocumentSession { get; }

        // We can add more of the send overloads, or we could expose an implementation of IServiceBus
        void Send<T>(T message);
        Task Complete();
    }


    /// <summary>
    /// Alternative 2
    /// </summary>
    /// <remarks>This alternative requires the extra IDocumentSessionListener</remarks>
    public class MartenOutboxBus : IDisposable
    {
        private readonly Action _unregister;

        public MartenOutboxBus(IDocumentSession session, SessionCommitListener listener)
        {
            // register with the session to be notified after SaveChanges completes.
            _unregister = listener.RegisterCallbackAfterCommit(session, enqueueOutgoing);
        }

        // We can add more of the send overloads, or we could fully implement IServiceBus
        public void Send<T>(T message)
        {
            //TODO: use the message router to get envelopes and store them in the session.
            //TODO: hold on to those envelopes so we can enqueue them when the session completes
            //Question: should we configure Marten to store the outgoing envelopes in a separate collection from the incoming (including delayed) ones?
            // or is it enough to use an index over the destination and/or execution time headers?
        }

        private void enqueueOutgoing()
        {
            //TODO: notify the SendingAgent that these envelopes are ready to be delivered
        }

        public void Dispose()
        {
            _unregister();
        }
    }


    public class OrdersApp : JasperRegistry
    {
        public OrdersApp()
        {
            Include<MartenBackedPersistence>();

            // Whether or not our event is destined for a durable queue, it will be stored durably in the outbox because of the usage of an outbox when sending it.
            Publish.Message<OrderPlaced>().To("tcp://localhost:2345/durable");

            Transports.LightweightListenerAt(5432);

            Settings.Alter<StoreOptions>(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
            });
            Handlers.DisableConventionalDiscovery();
            Handlers.IncludeType<OrderStatusHandler>();
        }
    }

    public class Order
    {
        public Guid Id { get; set; }

        public string ItemName { get; set; }

        public string Status { get; set; }
    }

    public class OrderPlaced
    {
        public Guid Id;
        public string ItemName;
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


    public class WarehouseApp<THandler> : JasperRegistry
    {
        public WarehouseApp()
        {
            Include<MartenBackedPersistence>();

            Settings.Alter<StoreOptions>(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
            });

            //Note: whether or not our event is destined for a durable queue, it will be stored durably in the outbox because of the implementation of the handlers.
            Publish.Message<ItemOutOfStock>().To("tcp://localhost:5432/");

            Transports.DurableListenerAt(2345);

            Handlers.DisableConventionalDiscovery();
            Handlers.IncludeType<THandler>();
        }
    }

    /* Three different alternatives for handler implementations that make use of the outbox */

    /// <summary>
    /// By using MartenTransaction and cascading messages, I want the cascading messages to be stored in the outbox with the same document session
    /// This could be made to work with either alternative outbox. I think it's reasonable to always use the Marten outbox when the MartenTransaction attribute is applied.
    /// </summary>
    public class WarehouseHandler1
    {
        [MartenTransaction]
        public object Handle(OrderPlaced orderPlaced, IDocumentSession session)
        {
            session.Store(new PickList { Id = orderPlaced.Id});
            var outOfStockEvent = new ItemOutOfStock {OrderId = orderPlaced.Id, ItemName = orderPlaced.ItemName};

            // wait here for the test to inspect state.
            TestSynch.ProcessedOrderPlacedEvent.Set();
            TestSynch.WarehouseHandlerShouldContinueEvent.WaitOne(5.Seconds());

            return outOfStockEvent;
        }
    }

    /// <summary>
    /// By taking a dependency on the outbox implementation, all the bus messages sent through it 
    /// will be tracked in the session the application uses for its own data via outbox.DocumentSession.
    /// </summary>
    public class WarehouseHandler2
    {
        public void Handle(OrderPlaced orderPlaced, IMartenOutbox outbox)
        {
            outbox.DocumentSession.Store(new PickList {Id = orderPlaced.Id});
            outbox.Send(new ItemOutOfStock {OrderId = orderPlaced.Id, ItemName = orderPlaced.ItemName});

            // wait here for the test to inspect state.
            TestSynch.ProcessedOrderPlacedEvent.Set();
            TestSynch.WarehouseHandlerShouldContinueEvent.WaitOne(5.Seconds());

            outbox.Complete();
        }
    }

    /// <summary>
    /// By using MartenTransaction and a service bus, the messages sent on the bus should be 
    /// stored in the outbox with the same document session
    /// Instead of the standard ServiceBus implementation, this implementation of IServiceBus would pass Send calls to an outbox. But how would the code generation know?
    /// I don't prefer this option because the code generation will need know how to instantiate the right IServiceBus implementation.
    /// However, if somebody does declare a dependency on IServiceBus while they're using MartenTransaction, I'd prefer they get the outbox behavior
    /// </summary>
    public class WarehouseHandler3
    {
        [MartenTransaction]
        public void Handle(OrderPlaced orderPlaced, IDocumentSession session, IServiceBus bus)
        {
            session.Store(new PickList { Id = orderPlaced.Id });
            bus.Send(new ItemOutOfStock { OrderId = orderPlaced.Id, ItemName = orderPlaced.ItemName });

            // wait here for the test to inspect state.
            TestSynch.ProcessedOrderPlacedEvent.Set();
            TestSynch.WarehouseHandlerShouldContinueEvent.WaitOne(5.Seconds());
        }
    }

    public class PickList
    {
        public Guid Id { get; set; }
    }

    public class ItemOutOfStock
    {
        public Guid OrderId { get; set; }
        public string ItemName { get; set; }
    }
}
