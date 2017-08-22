# Simple Routing via Subscriptions

-> id = 4b44f85d-f43c-469d-bdb7-67c6cb2a9f16
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-08-22T13:45:45.7291200Z
-> tags = 

[Communication]
|> ForService
    [Node]
    |> ForService serviceName=Receiver1
    |> SubscribesTo messageType=Message1, port=2222

|> ForService
    [Node]
    |> ForService serviceName=Receiver2
    |> SubscribesTo messageType=Message1, port=2223
    |> SubscribesTo messageType=Message2, port=2223

|> SendMessage messageType=Message1, name=Bill
|> SendMessage messageType=Message1, name=Tom
|> SendMessage messageType=Message2, name=George
|> TheMessagesSentShouldBe
    [rows]
    |ServiceName|MessageType|Name  |
    |Receiver1  |Message1   |Bill  |
    |Receiver2  |Message1   |Bill  |
    |Receiver1  |Message1   |Tom   |
    |Receiver2  |Message1   |Tom   |
    |Receiver2  |Message2   |George|

~~~
