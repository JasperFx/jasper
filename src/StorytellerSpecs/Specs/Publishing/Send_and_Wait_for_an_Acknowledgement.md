# Send and Wait for an Acknowledgement

-> id = 820fa6a1-4a20-4220-9e0c-7700d4484b9b
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-03-21T22:25:57.3198615Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1, channel=stub://one
    |> SendMessage messageType=ErrorMessage, channel=stub://one


Happy Path

|> SendMessageSuccessfully
|> AckIsReceived
|> AckWasSuccessful

Sad Path

|> SendMessageUnsuccessfully
|> AckIsReceived
|> TheAckFailedWithMessage message=AmbiguousMatchException
~~~
