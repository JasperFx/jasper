# Only keep the retry storage limit when getting failures

-> id = c9fd5a2a-3eb6-4da7-9dda-99bb21829c98
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2018-07-09T12:36:14.7095530Z
-> tags = 

[RetryAgent]
|> FailuresBeforeCircuitBreaks count=3
|> CooldownIs50Ms
|> MaximumEnvelopeRetryStorage number=200
|> PingingIsFailing
|> BatchFails count=50
|> BatchFails count=35
|> BatchFails count=25
|> BatchFails count=55
|> BatchFails count=43
|> BatchFails count=78
|> BatchFails count=50
|> BatchFails count=32
|> QueuedCount count=200
~~~
