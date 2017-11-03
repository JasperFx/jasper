# Messages Sent to Multiple Channels

-> id = c553e31b-702b-4c38-991b-4eb2c28f2425
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-11-03T19:20:27.1093270Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1
    ``` channel
    durable://localhost:2201/one
    ```

    |> SendMessage messageType=Message2
    ``` channel
    durable://localhost:2201/two
    ```

    |> SendMessage messageType=Message3
    ``` channel
    durable://localhost:2201/three
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
