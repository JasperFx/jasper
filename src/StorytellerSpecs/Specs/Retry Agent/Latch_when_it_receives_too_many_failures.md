# Latch when it receives too many failures

-> id = 9533abab-5466-47e0-9940-549ff1042f17
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2018-07-09T12:53:01.4675190Z
-> tags = 

[RetryAgent]
|> PingingIsFailing
|> FailuresBeforeCircuitBreaks count=3
|> BatchFails count=5
|> BatchFails count=5
|> BatchFails count=5
|> BatchFails count=5
|> TheSenderWasLatched
~~~
