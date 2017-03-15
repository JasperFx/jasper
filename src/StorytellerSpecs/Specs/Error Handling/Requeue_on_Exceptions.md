# Requeue on Exceptions

-> id = 8810efb4-0560-4807-bca1-f62bcbb99dbb
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-03-15T15:18:49.8868580Z
-> tags = 

[ErrorHandling]

First, what about the case where the maximum attempts is capped at 1?

|> IfTheChainHandlingIs
    [ChainErrorHandling]
    |> MaximumAttempts attempts=1
    |> RequeueOn errorType=DivideByZeroException

|> MessageAttempts
    [Rows]
    |> MessageAttempts-row Attempt=1, errorType=DivideByZeroException


Does not matter that it's configured to retry the message on that exception because the attempts are capped

|> MessageResult attempt=1, result=MovedToErrorQueue

Now, move the maximum attempts up

|> IfTheChainHandlingIs
    [ChainErrorHandling]
    |> MaximumAttempts attempts=3
    |> RequeueOn errorType=DivideByZeroException

|> MessageAttempts
    [Rows]
    |> MessageAttempts-row Attempt=1, errorType=DivideByZeroException

|> MessageResult attempt=2, result=Succeeded
|> MessageAttempts
    [Rows]
    |Attempt|errorType            |
    |1      |DivideByZeroException|
    |2      |DivideByZeroException|

|> MessageResult attempt=3, result=Succeeded
|> MessageAttempts
    [Rows]
    |Attempt|errorType            |
    |1      |DivideByZeroException|
    |2      |DivideByZeroException|
    |3      |DivideByZeroException|

|> MessageResult attempt=3, result=MovedToErrorQueue
~~~
