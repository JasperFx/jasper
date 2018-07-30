# Sending Resumes when the Receiver is Detected

-> id = 204bb3d3-bbf7-409a-b959-ab27bad87032
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-12-01T13:55:27.9111770Z
-> tags = 

[MartenBackedPersistence]
|> StartSender name=Sender1
|> SendMessages sender=Sender1, count=5
|> StartReceiver name=Receiver1
|> WaitForMessagesToBeProcessed count=5
|> PersistedIncomingCount count=0
|> PersistedOutgoingCount count=0
|> ReceivedMessageCount count=5
~~~
