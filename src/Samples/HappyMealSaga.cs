using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper;
using Jasper.Persistence.Sagas;

namespace Samples
{
    // SAMPLE: HappyMealOrderState
    public class HappyMealOrderState
    {
        // Jasper wants you to make the saga state
        // document have an "Id" property, but
        // that can be overridden
        public int Id { get; set; }
        public HappyMealOrder Order { get; set; }

        public bool DrinkReady { get; set; }
        public bool ToyReady { get; set; }
        public bool SideReady { get; set; }
        public bool MainReady { get; set; }

        // The order is complete if *everything*
        // is complete
        public bool IsOrderComplete()
        {
            return DrinkReady && ToyReady && SideReady && MainReady;
        }
    }
    // ENDSAMPLE

    // SAMPLE: HappyMealOrder
    public class HappyMealOrder
    {
        public string Drink { get; set; }
        public string Toy { get; set; }
        public string SideDish { get; set; }
        public string MainDish { get; set; }
    }
    // ENDSAMPLE

    public class FetchDrink
    {
        public string DrinkName { get; set; }
    }

    public class FetchFries
    {
    }

    public class FetchToy
    {
        public string ToyName { get; set; }
    }

    public class MakeHamburger
    {
    }

    public class FetchChickenNuggets
    {
    }

    public class SodaRequested
    {
        public int OrderId { get; set; }
    }


    // SAMPLE: HappyMealSaga1
    /// <summary>
    ///     This is being done completely in memory, which you most likely wouldn't
    ///     do in "real" systems
    /// </summary>
    public class HappyMealSaga : StatefulSagaOf<HappyMealOrderState>
    {
        private int _orderIdSequence;

        // This is a little bit cute, but the HappyMealOrderState type
        // is known to be the saga state document, so it'll be treated as
        // the state document, while the object[] will be treated as
        // cascading messages
        public (HappyMealOrderState, object[]) Starts(HappyMealOrder order)
        {
            var state = new HappyMealOrderState
            {
                Order = order,
                Id = ++_orderIdSequence
            };

            return (state, chooseActions(order, state.Id).ToArray());
        }

        private IEnumerable<object> chooseActions(HappyMealOrder order, int stateId)
        {
            // choose the outgoing messages to other systems -- or the local
            // system tracking all this -- to start having this happy meal
            // order put together

            if (order.Drink == "Soda") yield return new SodaRequested {OrderId = stateId};

            // and others
        }
    }
    // ENDSAMPLE


    public class HappyMealSagaNoTuple : StatefulSagaOf<HappyMealOrderState>
    {
        private int _orderIdSequence;

        // SAMPLE: HappyMealSaga1NoTuple

        public async Task<HappyMealOrderState> Starts(
            HappyMealOrder order, // The first argument is assumed to be the message type
            IExecutionContext context) // Additional arguments are assumed to be services
        {
            var state = new HappyMealOrderState
            {
                Order = order,
                Id = ++_orderIdSequence
            };

            if (order.Drink == "Soda") await context.Send(new SodaRequested {OrderId = state.Id});

            // And other outgoing messages to coordinate gathering up the happy meal

            return state;
        }

        // ENDSAMPLE
    }

    public class HappyMealSagaAllLocal : StatefulSagaOf<HappyMealOrderState>
    {
        private int _orderIdSequence;

        // SAMPLE: HappyMealSaga1Local

        public async Task<HappyMealOrderState> Starts(
            HappyMealOrder order, // The first argument is assumed to be the message type
            IExecutionContext context) // Additional arguments are assumed to be services
        {
            var state = new HappyMealOrderState
            {
                Order = order,
                Id = ++_orderIdSequence
            };

            if (order.Drink == "Soda") await context.Enqueue(new SodaRequested {OrderId = state.Id});

            // And other outgoing messages to coordinate gathering up the happy meal

            return state;
        }

        // ENDSAMPLE
    }

    // SAMPLE: SodaHandler
    // This message handler is in another system responsible for
    // filling sodas
    public class SodaHandler
    {
        public SodaFetched Handle(SodaRequested requested)
        {
            // get the soda, then return the update message
            return new SodaFetched();
        }
    }
    // ENDSAMPLE

    /// <summary>
    ///     This is being done completely in memory, which you most likely wouldn't
    ///     do in "real" systems
    /// </summary>
    public class HappyMealSaga2 : StatefulSagaOf<HappyMealOrderState>
    {
        private int _orderIdSequence;

        // This is a little bit cute, but the HappyMealOrderState type
        // is known to be the saga state document, so it'll be treated as
        // the state document, while the object[] will be treated as
        // cascading messages
        public async Task<HappyMealOrderState> Starts(HappyMealOrder order, IExecutionContext context)
        {
            var state = new HappyMealOrderState
            {
                Order = order,
                Id = ++_orderIdSequence
            };


            // You can explicitly call the IMessageContext if you prefer
            if (order.Drink == "Soda") await context.Send(new SodaRequested {OrderId = state.Id});


            return state;
        }
    }

    public interface IOrderService
    {
        void Close(int order);
    }

    public class SodaFetched
    {
    }

    // SAMPLE: BurgerReady
    public class BurgerReady
    {
        // By default, Jasper is going to look for a property
        // called SagaId as the identifier for the stateful
        // document
        public int SagaId { get; set; }
    }
    // ENDSAMPLE

    // SAMPLE: ToyOnTray
    public class ToyOnTray
    {
        // There's always *some* reason to deviate,
        // so you can use this attribute to tell Jasper
        // that this property refers to the Id of the
        // Saga state document
        [SagaIdentity] public int OrderId { get; set; }
    }
    // ENDSAMPLE


    public class HappyMealSaga3 : StatefulSagaOf<HappyMealOrderState>
    {
        // SAMPLE: completing-saga
        public void Handle(
            SodaFetched soda, // The first argument is the message type
            IOrderService service // Additional arguments are injected services
        )
        {
            State.DrinkReady = true;

            // Determine if the happy meal is completely ready
            if (State.IsOrderComplete())
            {
                // Maybe you need to remove this
                // order from some kind of screen display
                service.Close(State.Id);

                // And we're done here, so let's mark the Saga as complete
                MarkCompleted();
            }
        }
        // ENDSAMPLE


        // SAMPLE: passing-saga-state-id-through-message
        public void Handle(ToyOnTray toyReady)
        {
            State.ToyReady = true;
            if (State.IsOrderComplete()) MarkCompleted();
        }

        public void Handle(BurgerReady burgerReady)
        {
            State.MainReady = true;
            if (State.IsOrderComplete()) MarkCompleted();
        }

        // ENDSAMPLE
    }
}
