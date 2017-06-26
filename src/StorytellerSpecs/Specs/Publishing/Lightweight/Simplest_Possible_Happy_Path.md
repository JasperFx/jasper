# Simplest Possible Happy Path

-> id = 63d1dfc5-e1bc-49fc-8a88-5d846f855c6d
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-03-20T20:33:18.1508239Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1
    ``` channel
    jasper://localhost:2201/one
    ```

    |> SendMessage messageType=Message2
    ``` channel
    jasper://localhost:2201/two
    ```

    |> SendMessage messageType=Message3
    ``` channel
    jasper://localhost:2201/three
    ```


|> SendMessage messageType=Message1, name=Tom
|> SendMessage messageType=Message2, name=Todd
|> SendMessage messageType=Message3, name=Trevor
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt                   |MessageType|Name  |
    |jasper://localhost:2201/one  |Message1   |Tom   |
    |jasper://localhost:2201/two  |Message2   |Todd  |
    |jasper://localhost:2201/three|Message3   |Trevor|

~~~
