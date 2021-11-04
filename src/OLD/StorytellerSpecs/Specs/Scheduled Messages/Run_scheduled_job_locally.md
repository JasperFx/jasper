# Run scheduled job locally

-> id = 42420729-1ff1-406e-b099-b723b7da5d01
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2018-07-09T13:18:41.6257950Z
-> tags = 

[ScheduledJob]
|> ScheduleMessage id=1, seconds=7200
|> ScheduleMessage id=2, seconds=5
|> ScheduleMessage id=3, seconds=7200
|> ReceivedMessageCount count=0
|> AfterReceivingMessages
|> TheIdOfTheOnlyReceivedMessageShouldBe id=2
~~~
