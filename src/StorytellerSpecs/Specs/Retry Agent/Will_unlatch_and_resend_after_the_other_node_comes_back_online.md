# Will unlatch and resend after the other node comes back online

-> id = 2527ef51-04ba-44ce-b258-13f20cb10711
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2018-07-09T12:30:06.7126020Z
-> tags = 

[RetryAgent]
|> FailuresBeforeCircuitBreaks count=3
|> CooldownIs50Ms
|> MarkFailed index=1
|> MarkFailed index=2
|> MarkFailed index=3

Should resume sending after it receives a successful ping

|> WaitForQueuedToBeZero
|> TheSenderWasUnlatched
|> TheBatchWasQueued index=1
|> TheBatchWasQueued index=2
|> TheBatchWasQueued index=3
|> QueuedCount count=0
~~~
