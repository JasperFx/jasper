# Simplest Possible Happy Path

-> id = 63d1dfc5-e1bc-49fc-8a88-5d556f847c6d
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-06-26T14:32:22.2023050Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1
    ``` channel
    lq.tcp://localhost:2201/one
    ```

    |> SendMessage messageType=Message2
    ``` channel
    lq.tcp://localhost:2201/two
    ```

    |> SendMessage messageType=Message3
    ``` channel
    lq.tcp://localhost:2201/three
    ```


|> SendMessage messageType=Message1, name=Tom
|> SendMessage messageType=Message2, name=Todd
|> SendMessage messageType=Message3, name=Trevor
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt                   |MessageType|Name  |
    |lq.tcp://localhost:2201/one  |Message1   |Tom   |
    |lq.tcp://localhost:2201/two  |Message2   |Todd  |
    |lq.tcp://localhost:2201/three|Message3   |Trevor|

~~~
