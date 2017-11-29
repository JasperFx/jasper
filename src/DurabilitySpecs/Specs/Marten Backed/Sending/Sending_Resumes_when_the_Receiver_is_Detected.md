# Sending Resumes when the Receiver is Detected

-> id = 204bb3d3-bbf7-409a-b959-ab27bad87032
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-11-29T15:54:48.2140680Z
-> tags = 

[MartenBackedPersistence]
|> StartSender name=Sender1
|> SendMessages sender=Sender1, count=100
|> StartReceiver name=Receiver1
|> WaitForSenderToBeUnlatched
|> WaitForMessagesToBeProcessed count=100
|> PersistedIncomingCount count=0
|> PersistedOutgoingCount count=0
|> ReceivedMessageCount count=100
~~~
