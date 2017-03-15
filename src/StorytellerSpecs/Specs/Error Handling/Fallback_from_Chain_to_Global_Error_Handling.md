# Fallback from Chain to Global Error Handling

-> id = aab39c33-410b-401e-9a87-5514284e8c15
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-03-15T19:28:03.6538526Z
-> tags = 

[ErrorHandling]
|> IfTheGlobalHandlingIs
    [GlobalErrorHandling]
    |> RetryOn errorType=DivideByZeroException
    |> RequeueOn errorType=DataMisalignedException
    |> MoveToErrorQueue errorType=InvalidOperationException

|> IfTheChainHandlingIs
    [ChainErrorHandling]
    |> MoveToErrorQueue errorType=DivideByZeroException
    |> RetryOn errorType=InvalidOperationException
    |> MaximumAttempts attempts=3


No chain specific rules, so use the global error handling

|> MessageAttempts
    [Rows]
    |Attempt|errorType              |
    |1      |DataMisalignedException|
    |2      |DataMisalignedException|

|> MessageResult attempt=3, result=Succeeded

The chain catches the exception with its specific rules

|> MessageAttempts
    [Rows]
    |> MessageAttempts-row Attempt=1, errorType=DivideByZeroException

|> MessageResult attempt=1, result=MovedToErrorQueue

Another case where the chain specific handling overrides

|> MessageAttempts
    [Rows]
    |> MessageAttempts-row Attempt=1, errorType=InvalidOperationException

|> MessageResult attempt=2, result=Succeeded
~~~
