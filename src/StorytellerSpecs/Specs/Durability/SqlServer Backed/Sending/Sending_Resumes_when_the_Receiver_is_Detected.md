# Sending Resumes when the Receiver is Detected

-> id = 504bb3d3-bbf7-409a-b959-ab27bad87032
-> lifecycle = Regression
-> max-retries = 2
-> last-updated = 2019-01-08T21:34:42.4060870Z
-> tags = 

[SqlServerBackedPersistence]
|> StartSender name=Sender1
|> SendMessages sender=Sender1, count=5
|> StartReceiver name=Receiver1
|> WaitForMessagesToBeProcessed count=5
|> PersistedIncomingCount count=0
|> PersistedOutgoingCount count=6
|> ReceivedMessageCount count=5
~~~
