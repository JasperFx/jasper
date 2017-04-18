# Multiple Cascading Messages In Memory

-> id = 1a2c9ed4-16df-4bc4-9ecc-988c03b6901b
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-04-18T20:26:15.2112014Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1
    ``` channel
    memory://localhost:2201/one
    ```

    |> SendMessage messageType=Message2
    ``` channel
    memory://localhost:2201/two
    ```

    |> SendMessage messageType=Message3
    ``` channel
    memory://localhost:2201/three
    ```

    |> SendMessage messageType=Message4
    ``` channel
    memory://localhost:2201/four
    ```

    |> ReceivingMessage2CascadesMultiples

|> SendMessage messageType=Message2, name=Tamba Hali
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt                   |MessageType|Name      |
    |memory://localhost:2201/two  |Message2   |Tamba Hali|
    |memory://localhost:2201/three|Message3   |Tamba Hali|
    |memory://localhost:2201/four |Message4   |Tamba Hali|

~~~
