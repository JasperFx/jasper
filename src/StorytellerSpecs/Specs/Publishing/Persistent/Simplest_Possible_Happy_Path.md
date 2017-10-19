# Simplest Possible Happy Path

-> id = 63d1dfc5-e1bc-49fc-8a88-5d556f847c6d
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-10-19T18:27:36.7701000Z
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
    |ReceivedAt              |MessageType|Name  |
    |durable://localhost:2201|Message1   |Tom   |
    |durable://localhost:2201|Message2   |Todd  |
    |durable://localhost:2201|Message3   |Trevor|

~~~
