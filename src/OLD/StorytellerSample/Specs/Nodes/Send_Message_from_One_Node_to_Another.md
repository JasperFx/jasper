# Send Message from One Node to Another

-> id = 4f4b969b-64a7-4938-aa17-2ccab2549c8e
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2018-04-04T12:47:50.1664730Z
-> tags = 

[Increment]
|> SendIncrementMessage
|> SendIncrementMessage
|> SendIncrementMessage
|> SendIncrementMessage
|> TheIncrementCountShouldBe count=4
|> SendIncrementMessage
|> TheIncrementCountShouldBe count=5
~~~
