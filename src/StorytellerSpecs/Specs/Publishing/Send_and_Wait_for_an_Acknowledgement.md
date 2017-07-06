# Send and Wait for an Acknowledgement

-> id = 820fa6a1-4a20-4220-9e0c-7700d4484b9b
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-07-06T18:26:22.2482512Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1, channel=memory://one/
    |> SendMessage messageType=ErrorMessage, channel=memory://one/
    |> ListenForMessagesFrom channel=memory://one/


Happy Path

|> SendMessageSuccessfully
|> AckIsReceived
|> AckWasSuccessful

Sad Path

|> SendMessageUnsuccessfully
|> AckIsReceived
|> TheAckFailedWithMessage message=AmbiguousMatchException
~~~
