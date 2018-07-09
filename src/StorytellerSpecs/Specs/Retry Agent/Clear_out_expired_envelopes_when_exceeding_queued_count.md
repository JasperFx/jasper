# Clear out expired envelopes when exceeding queued count

-> id = 5e331495-2f7c-4dd1-8303-302cbd8a9a5a
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2018-07-09T12:48:50.6148520Z
-> tags = 

[RetryAgent]
|> MaximumEnvelopeRetryStorage number=200
|> PingingIsFailing
|> BatchFailsWithExpired count=100, expired=25
|> BatchFailsWithExpired count=50, expired=25
|> BatchFailsWithExpired count=100, expired=10
|> BatchFailsWithExpired count=100, expired=10
|> BatchFailsWithExpired count=100, expired=10
|> QueuedCount count=200
|> NoQueuedEnvelopesAreExpired
~~~
