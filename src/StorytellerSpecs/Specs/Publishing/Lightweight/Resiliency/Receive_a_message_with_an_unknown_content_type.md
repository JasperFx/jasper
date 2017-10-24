# Receive a message with an unknown content type

-> id = b9cffe0d-2b61-48eb-b5fd-7b1f5eec522a
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-10-19T18:26:51.0264640Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> ListenForMessagesFrom channel=tcp://localhost:2201/one
    |> SendMessage messageType=Message1, channel=tcp://localhost:2201/one
    |> SendMessage messageType=Message2, channel=tcp://localhost:2201/one

|> SendMessageWithUnknownContentType address=tcp://localhost:2201/one
|> SendMessage messageType=Message1, name=Suzy
|> SendMessage messageType=Message2, name=Russell
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt          |MessageType|Name   |
    |tcp://localhost:2201|Message1   |Suzy   |
    |tcp://localhost:2201|Message2   |Russell|

~~~
