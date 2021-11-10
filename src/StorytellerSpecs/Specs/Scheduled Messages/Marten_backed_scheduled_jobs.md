# Marten backed scheduled jobs

-> id = 3a808649-f9a8-40bf-acb4-b3f133a2b857
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2021-11-10T12:13:24.7633724Z
-> tags = 

[MartenScheduledJob]
|> ScheduleSendMessage id=1, seconds=7200
|> ScheduleSendMessage id=2, seconds=5
|> ScheduleSendMessage id=3, seconds=7200
|> ReceivedMessageCount count=0
|> AfterReceivingMessages
|> TheIdOfTheOnlyReceivedMessageShouldBe id=2
|> PersistedScheduledCountShouldBe expected=2
~~~
