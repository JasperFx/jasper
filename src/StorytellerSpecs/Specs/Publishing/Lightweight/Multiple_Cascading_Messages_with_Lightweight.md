# Multiple Cascading Messages with Lightweight

-> id = 75de41e9-02d0-41d0-be5f-df8d67f1a764
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-06-26T14:22:39.3652280Z
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

    |> SendMessage messageType=Message4
    ``` channel
    jasper://localhost:2201/four
    ```

    |> ReceivingMessage2CascadesMultiples
    |> ListenForMessagesFrom
    ``` channel
    jasper://localhost:2201/one
    ```

    |> ListenForMessagesFrom
    ``` channel
    jasper://localhost:2201/two
    ```

    |> ListenForMessagesFrom
    ``` channel
    jasper://localhost:2201/three
    ```

    |> ListenForMessagesFrom
    ``` channel
    jasper://localhost:2201/four
    ```


|> SendMessage messageType=Message2, name=Tamba Hali
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt                   |MessageType|Name      |
    |jasper://localhost:2201/two  |Message2   |Tamba Hali|
    |jasper://localhost:2201/three|Message3   |Tamba Hali|
    |jasper://localhost:2201/four |Message4   |Tamba Hali|

~~~
