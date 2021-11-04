# Send scheduled messages

-> id = a37059bf-30f9-47da-8d7d-8e5c29e71bc9
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2018-07-09T13:21:23.9528100Z
-> tags = 

[ScheduledJob]
|> ScheduleSendMessage id=1, seconds=7200
|> ScheduleSendMessage id=2, seconds=5
|> ScheduleSendMessage id=3, seconds=7200
|> ReceivedMessageCount count=0
|> AfterReceivingMessages
|> TheIdOfTheOnlyReceivedMessageShouldBe id=2
~~~
