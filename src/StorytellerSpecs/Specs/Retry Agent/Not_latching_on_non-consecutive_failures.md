# Not latching on non-consecutive failures

-> id = d68fdcb4-09b8-4c4f-92ae-f35b13de1a90
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2018-07-09T12:22:47.6970790Z
-> tags = 

[RetryAgent]
|> FailuresBeforeCircuitBreaks count=3
|> MarkFailed index=0
|> MarkFailed index=1
|> MarkSuccess
|> MarkFailed index=2
|> MarkFailed index=3
|> TheSenderWasNotLatched
|> TheBatchWasQueued index=0
|> TheBatchWasQueued index=1
|> TheBatchWasQueued index=2
|> TheBatchWasQueued index=3
~~~
