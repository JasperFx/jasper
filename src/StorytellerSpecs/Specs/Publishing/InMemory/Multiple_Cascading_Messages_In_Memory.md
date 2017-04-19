# Multiple Cascading Messages In Memory

-> id = 1a2c9ed4-16df-4bc4-9ecc-988c03b6901b
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-04-18T21:35:00.2663393Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1
    ``` channel
    memory://one
    ```

    |> SendMessage messageType=Message2
    ``` channel
    memory://two
    ```

    |> SendMessage messageType=Message3
    ``` channel
    memory://three
    ```

    |> SendMessage messageType=Message4
    ``` channel
    memory://four
    ```

    |> ReceivingMessage2CascadesMultiples

|> SendMessage messageType=Message2, name=Tamba Hali
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt    |MessageType|Name      |
    |memory://two  |Message2   |Tamba Hali|
    |memory://three|Message3   |Tamba Hali|
    |memory://four |Message4   |Tamba Hali|

~~~
