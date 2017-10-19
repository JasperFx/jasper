# Retry Later Mechanics

-> id = cfb9c3d0-c729-4ae8-b7d1-72e3cec9da44
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-10-19T19:24:15.9055860Z
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
~~~
