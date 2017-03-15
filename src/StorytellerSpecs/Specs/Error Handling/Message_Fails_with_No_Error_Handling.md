# Message Fails with No Error Handling

-> id = 9fc35088-676a-4c61-a445-5316875bafa5
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-03-15T15:01:06.3821600Z
-> tags = 

[ErrorHandling]
|> MessageAttempts
    [Rows]
    |> MessageAttempts-row Attempt=1, errorType=DivideByZeroException

|> MessageResult attempt=1, result=MovedToErrorQueue
~~~
