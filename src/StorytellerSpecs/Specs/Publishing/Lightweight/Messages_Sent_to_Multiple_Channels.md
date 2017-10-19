# Messages Sent to Multiple Channels

-> id = c553e31b-702b-4c38-991b-4eb2c28f2424
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-10-19T15:36:28.6946270Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1, channel=tcp://localhost:2201/one
    |> SendMessage messageType=Message2, channel=tcp://localhost:2201/two
    |> SendMessage messageType=Message3
    ``` channel
    tcp://localhost:2201/three
    ```


|> SendMessage messageType=Message1, name=Tom
|> SendMessage messageType=Message2, name=Todd
|> SendMessage messageType=Message3, name=Trevor
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt          |MessageType|Name  |
    |tcp://localhost:2201|Message1   |Tom   |
    |tcp://localhost:2201|Message2   |Todd  |
    |tcp://localhost:2201|Message3   |Trevor|

~~~
