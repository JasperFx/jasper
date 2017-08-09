# Simplest Possible Happy Path

-> id = 2d261ec7-b103-4a20-a900-778ea0be1e13
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-04-18T20:33:19.1508239Z
-> tags =

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1
    ``` channel
    loopback://one
    ```

    |> SendMessage messageType=Message2
    ``` channel
    loopback://two
    ```

    |> SendMessage messageType=Message3
    ``` channel
    loopback://three
    ```


|> SendMessage messageType=Message1, name=Tom
|> SendMessage messageType=Message2, name=Todd
|> SendMessage messageType=Message3, name=Trevor
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt                   |MessageType|Name  |
    |loopback://one  |Message1   |Tom   |
    |loopback://two  |Message2   |Todd  |
    |loopback://three|Message3   |Trevor|

~~~
