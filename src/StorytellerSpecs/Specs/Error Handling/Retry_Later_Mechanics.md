# Retry Later Mechanics

-> id = cfb9c3d0-c729-4ae8-b7d1-72e3cec9da44
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-03-15T15:35:02.7758200Z
-> tags = 

[ErrorHandling]

First, try it with the maximum attempts set to 1 so it shouldn't do any kind of retry

|> IfTheChainHandlingIs
    [ChainErrorHandling]
    |> MaximumAttempts attempts=1
    |> RetryLater errorType=DivideByZeroException

|> MessageAttempts
    [Rows]
    |> MessageAttempts-row Attempt=1, errorType=DivideByZeroException

|> MessageResult attempt=1, result=MovedToErrorQueue

Now, try it again with some allowable retries

|> IfTheChainHandlingIs
    [ChainErrorHandling]
    |> MaximumAttempts attempts=3
    |> RetryLater errorType=DivideByZeroException

|> MessageAttempts
    [Rows]
    |> MessageAttempts-row Attempt=1, errorType=DivideByZeroException

|> MessageResult attempt=1, result=Retry in 5 seconds
~~~
