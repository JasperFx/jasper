# Sending Recovered Messages when Sender Starts Up

-> id = ee2f1d5a-3c77-4ea6-8f1f-8d788efc017e
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-12-01T13:55:36.9710820Z
-> tags = 

[MartenBackedPersistence]
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
