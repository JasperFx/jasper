# Sending Resumes when the Receiver is Detected

-> id = 204bb3d3-bbf7-409a-b959-ab27bad87032
-> lifecycle = Regression
-> max-retries = 3
-> last-updated = 2021-11-10T14:51:25.9125276Z
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
