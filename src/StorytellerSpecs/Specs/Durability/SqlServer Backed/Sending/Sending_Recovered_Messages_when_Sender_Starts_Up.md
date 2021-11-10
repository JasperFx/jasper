# Sending Recovered Messages when Sender Starts Up

-> id = fe2f1d5a-3c77-4ea6-8f1f-8d788efc017e
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2018-08-06T00:55:43.9667220Z
-> tags = 

[SqlServerBackedPersistence]
|> StartSender name=Sender1
|> SendMessages sender=Sender1, count=10
|> StopSender name=Sender1
|> PersistedOutgoingCount count=10
|> StartReceiver name=Receiver1
|> StartSender name=Sender2
|> WaitForMessagesToBeProcessed count=10
|> PersistedIncomingCount count=0
|> PersistedOutgoingCount count=0
|> ReceivedMessageCount count=10
~~~
