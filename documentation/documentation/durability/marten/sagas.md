<!--title:Marten Backed Saga Persistence-->

See <[linkto:documentation/messaging/sagas]> for an introduction to stateful sagas within Jasper.

To use [Marten](http://jasperfx.github.io/marten) as the backing store for saga persistence, start by enabling
the <[linkto:documentation/extensions/marten/persistence;title=Marten-backed messaging persistence]> like this:

<[sample:SagaApp-with-Marten]>

Any message handlers within a `StatefulSagaOf<T>` class will automatically have the <[linkto:documentation/extensions/marten/middleware;title=transactional middleware]>
applied. The limitation here is that you have to allow Jasper.Marten to handle all the transactional boundaries.

The saga state documents are all persisted as Marten documents.