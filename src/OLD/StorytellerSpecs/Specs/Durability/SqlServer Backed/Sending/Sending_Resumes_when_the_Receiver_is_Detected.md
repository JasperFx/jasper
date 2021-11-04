# Sending Resumes when the Receiver is Detected

-> id = 504bb3d3-bbf7-409a-b959-ab27bad87032
-> lifecycle = Regression
-> max-retries = 2
-> last-updated = 2019-03-12T01:36:07.4023390Z
-> tags = 

[SqlServerBackedPersistence]
|> StartSender name=Sender1
|> SendMessages sender=Sender1, count=5
|> StartReceiver name=Receiver1
|> WaitForMessagesToBeProcessed count=5
|> PersistedIncomingCount count=0
|> PersistedOutgoingCount count=0
|> ReceivedMessageCount count=5
~~~
