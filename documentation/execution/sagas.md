<!--title:Stateful Sagas-->


As is so common in these docs, I would direct you to this from the old "EIP" book: [Process Manager](http://www.enterpriseintegrationpatterns.com/patterns/messaging/ProcessManager.html). A stateful saga in Jasper is used
to coordinate long running workflows or to break large, logical transactions into a series of smaller steps. A stateful saga
consists of a couple parts:

1. A saga state document type that is persisted between saga messages
1. A saga message handler that inherits from `StatefulSagaOf<T>`, where the "T" is the saga state document type
1. A saga persistence strategy registered in Jasper that knows how to load and persist the saga state documents

Right now the only options for saga persistence are the default in memory model and an option that uses <[linkto:extensions/marten/sagas;title=Marten and Postgresql]> for
persistence. Other options are planned for [Entity Framework](https://github.com/JasperFx/jasper/issues/363) and [Dapper](https://github.com/JasperFx/jasper/issues/362).

Inspired by [Jimmy Bogard's example of ordering at a fast food restaurant](https://lostechies.com/jimmybogard/2013/03/14/saga-implementation-patterns-controller/), let's say that we are building 
a Jasper system to manage filling the order of Happy Meals at McDonald's. When you place an order in our McDonald's, various folks gather up the various parts of the order until it is completed, then calls out to the customer that the order is ready. In our system,
we want to build out a saga handler for this work that can parallelize and coordinate all the work necessary to deliver the Happy Meal
to our customers.

As a first step, let's say that the process of building out a Happy Meal starts with receiving this message shown below:

<[sample:HappyMealOrder]>

Next, we need to track the current state of our happy meal order with a new document type that will be persisted across different 
messages being handled by our saga:

<[sample:HappyMealOrderState]>

Finally, let's add a new saga handler for our new state document and a single message handler action to start the saga:

<[sample:HappyMealSaga1]>

There's a couple things to note about the `Starts()` method up above:

* When Jasper sees a method named `Start` or `Starts`, that indicates to Jasper that this is a possible beginning for a stateful saga and that the state document does not already exist
* `HappyMealOrderState` is one of the types returned as a C# 7 tuple from this method, and Jasper will treat this as the state document
for the saga and the saga persistence will save the document as part of executing that message. You could also just return the `HappyMealOrderState` and use `IMessageContext` to explicitly send other messages
* The `object[]` part of the tuple are just <[linkto:handling/cascading;title=cascading messages]> that would be sent out to initiate work like "go fill the soda" or "go find the right toy the child asked for"

If you're uncomfortable with C# tuples or just don't like the magic, you can effect the same outgoing messages by using this alternative:

<[sample:HappyMealSaga1NoTuple]>

Both the of the examples above assume that the `SodaRequested` messages will be sent to other systems, but it's perfectly possible
to use a stateful saga to manage processing that's handled completely within your system like this:

<[sample:HappyMealSaga1Local]>

## Updating and Completing Saga State

As we saw above, methods named `Start` or `Starts` are assumed to create a brand new state document for a logical saga. The next step is to handle additional methods that perform additional work within the saga, update the state document, and potentially close out the saga.

Related to the saga above, let's say that we receive a `SodaFetched` message that denotes that the soda we requested at the onset of the saga is ready. We'll need to update the state to mark that the drink is ready, and if all the parts of the happy meal are ready, we'll tell some kind of `IOrderService` to close the order and mark the saga as complete. That handler mthod could look like this:

<[sample:completing-saga]>

Things to note in the sample up above:

* The method name can be any of the valid handler methods outlined in <[linkto:handling/discovery]>
* The saga state document `HappyMealOrderState` is passed in as a method argument. The surrounding saga persistence in Jasper is
  loading that document for you based on either the incoming envelope metadata or the message as explained in the next section
* If the `StatefulSagaOf<T>.MarkCompleted()` method is called while handling the message, the state document will be deleted
  from storage after the message handler is called


## Saga State Identity

One way or another, Jasper has to be able to correlate that the `SodaFetched` message coming in to the message handler above
is related to a certain persisted state document by the id of that state document. Jasper has a couple ways to do that:

If the message is being passed to the local system or exchanging messages with an external Jasper system, there's an `Envelope.SagaId` property that gets propagated on every message and response that Jasper can use automatically to do the state document to message correlation. For example, if the `SodaRequested` message is sent to another system running Jasper that replies to our system with 
a corresponding `SodaFetched` message in a handler like this:

<[sample:SodaHandler]>

If you are receiving messages from an external system or don't want to or can't depend on that envelope metadata, you can pass the identity of the saga state document in the message itself. Jasper always checks the `Envelope.SagaId` value first, but failing that it falls back to looking for a property named `SagaId` on the incoming message type like this:

<[sample:BurgerReady]>

Or if you want to use a different property name, you can override that with an attribute like this:

<[sample:ToyOnTray]>

To add some context, let's see these two messages in context:

<[sample:passing-saga-state-id-through-message]>

If you were using the <[linkto:extensions/marten/sagas;title=Marten-backed saga persistence]>, the code above
would result in the `HappyMealOrderState` document being loaded with the value in `BurgerReady.SagaId` or `ToyOnTray.OrderId` as the document id.

Right now, Jasper supports the following types as valid saga state document identity types:

* `int`
* `long`
* `System.Guid`
* `string`

