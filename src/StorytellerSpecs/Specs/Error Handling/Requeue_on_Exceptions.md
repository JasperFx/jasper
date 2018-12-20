# Requeue on Exceptions

-> id = 8810efb4-0560-4807-bca1-f62bcbb99dbb
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-03-15T19:28:03.6958526Z
-> tags = 

[ErrorHandling]

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
