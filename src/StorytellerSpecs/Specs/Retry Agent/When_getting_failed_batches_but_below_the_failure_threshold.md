# When getting failed batches but below the failure threshold

-> id = bf2b2630-8b5a-4a0f-b70f-f22774143ef9
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2018-07-09T12:55:05.3321790Z
-> tags = 

[RetryAgent]
|> FailuresBeforeCircuitBreaks count=3
|> MarkFailed index=1
|> MarkFailed index=2
|> TheSenderWasNotLatched
|> TheBatchWasQueued index=1
|> TheBatchWasQueued index=2
|> QueuedCount count=0
~~~
