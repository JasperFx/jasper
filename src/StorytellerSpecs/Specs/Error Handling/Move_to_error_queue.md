# Move to error queue

-> id = c17eed13-96d2-49e5-a976-aa78f618081f
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-03-15T19:11:21.5568559Z
-> tags = 

[ErrorHandling]
|> IfTheChainHandlingIs
    [ChainErrorHandling]
    |> MaximumAttempts attempts=3
    |> MoveToErrorQueue errorType=DivideByZeroException
    |> RetryOn errorType=DataMisalignedException

|> MessageAttempts
    [Rows]
    |Attempt|errorType              |
    |1      |DataMisalignedException|
    |2      |DivideByZeroException  |

|> MessageResult attempt=2, result=MovedToErrorQueue
|> MessageAttempts
    [Rows]
    |Attempt|errorType              |
    |1      |DataMisalignedException|
    |2      |DataMisalignedException|
    |3      |ArgumentNullException  |

|> MessageResult attempt=3, result=MovedToErrorQueue
|> SendMessageWithNoErrors
|> MessageResult attempt=1, result=Succeeded
|> MessageAttempts
    [Rows]
    |> MessageAttempts-row Attempt=1, errorType=DivideByZeroException

|> MessageResult attempt=1, result=MovedToErrorQueue
~~~
