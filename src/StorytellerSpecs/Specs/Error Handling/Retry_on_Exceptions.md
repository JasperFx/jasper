# Retry on Exceptions

-> id = ca9ae70b-1211-42e0-8f55-0f6594c53dd5
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-03-15T19:28:03.7148526Z
-> tags = 

[ErrorHandling]

First, what about the case where the maximum attempts is capped at 1?

|> IfTheChainHandlingIs
    [ChainErrorHandling]
    |> MaximumAttempts attempts=1
    |> RetryOn errorType=DivideByZeroException

|> MessageAttempts
    [Rows]
    |> MessageAttempts-row Attempt=1, errorType=DivideByZeroException


Does not matter that it's configured to retry the message on that exception because the attempts are capped

|> MessageResult attempt=1, result=MovedToErrorQueue

Now, move the maximum attempts up

|> IfTheChainHandlingIs
    [ChainErrorHandling]
    |> MaximumAttempts attempts=3
    |> RetryOn errorType=DivideByZeroException

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
