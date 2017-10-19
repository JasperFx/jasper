# Simplest Possible Happy Path

-> id = 63d1dfc5-e1bc-49fc-8a88-5d846f847c6e
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-10-19T15:26:34.7288250Z
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

    |> ListenForMessagesFrom channel=tcp://localhost:2201/one
    |> ListenForMessagesFrom channel=tcp://localhost:2201/two
    |> ListenForMessagesFrom
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
