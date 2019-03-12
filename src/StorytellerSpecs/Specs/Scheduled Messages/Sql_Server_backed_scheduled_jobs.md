# Sql Server backed scheduled jobs

-> id = e7589592-0af1-473a-ba7e-497f5728ad0a
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2019-03-12T13:37:58.6298430Z
-> tags = 

[SqlServerScheduledJob]
|> ScheduleSendMessage id=1, seconds=7200
|> ScheduleSendMessage id=2, seconds=5
|> ScheduleSendMessage id=3, seconds=7200
|> ReceivedMessageCount count=0
|> AfterReceivingMessages
|> TheIdOfTheOnlyReceivedMessageShouldBe id=2
|> PersistedScheduledCount count=2
~~~
