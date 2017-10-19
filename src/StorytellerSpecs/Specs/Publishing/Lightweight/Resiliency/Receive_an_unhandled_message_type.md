# Receive an unhandled message type

-> id = 9387a12d-5d59-44dc-9add-02c51c49b29d
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-10-19T18:27:04.0471280Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> ListenForMessagesFrom channel=tcp://localhost:2201/one
    |> SendMessage messageType=Message1, channel=tcp://localhost:2201/one
    |> SendMessage messageType=Message2, channel=tcp://localhost:2201/one


There is no handler for UnhandledMessage in this configuration

|> SendMessageDirectly messageType=UnhandledMessage, name=Bill, address=tcp://localhost:2201/one
|> SendMessage messageType=Message1, name=Suzy
|> SendMessage messageType=Message2, name=Russell
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt          |MessageType|Name   |
    |tcp://localhost:2201|Message1   |Suzy   |
    |tcp://localhost:2201|Message2   |Russell|

~~~
