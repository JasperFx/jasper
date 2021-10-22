# TODO

DONE * Separate out a `MessagePublisher` from `MessageContext`
DONE * Rename `MessageContext` to `ExecutionContext`
DONE * Thin down continuations so all the logic is in `ExecutionContext`
* Unit tests against `ExecutionContext`
* `RabbitMqEnvelope` and stateless `RabbitMq
* Move the `EnlistInTransaction()` to some sort of common interface
  that is implemented by both `IMessagePublisher` and `IExecutionContext`
