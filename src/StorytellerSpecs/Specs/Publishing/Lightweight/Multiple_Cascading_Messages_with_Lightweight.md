# Multiple Cascading Messages with Lightweight

-> id = 75de41e9-02d0-41d0-be5f-df8d67f1a764
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-06-26T19:52:19.8207215Z
-> tags =

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1
    ``` channel
    tcp://localhost:2201/one
    ```

    |> SendMessage messageType=Message2
    ``` channel
    tcp://localhost:2201/two
    ```

    |> SendMessage messageType=Message3
    ``` channel
    tcp://localhost:2201/three
    ```

    |> SendMessage messageType=Message4
    ``` channel
    tcp://localhost:2201/four
    ```

    |> ReceivingMessage2CascadesMultiples
    |> ListenForMessagesFrom
    ``` channel
    tcp://localhost:2201/one
    ```

    |> ListenForMessagesFrom
    ``` channel
    tcp://localhost:2201/two
    ```

    |> ListenForMessagesFrom
    ``` channel
    tcp://localhost:2201/three
    ```

    |> ListenForMessagesFrom
    ``` channel
    tcp://localhost:2201/four
    ```


|> SendMessage messageType=Message2, name=Tamba Hali
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt                   |MessageType|Name      |
    |tcp://localhost:2201/two  |Message2   |Tamba Hali|
    |tcp://localhost:2201/three|Message3   |Tamba Hali|
    |tcp://localhost:2201/four |Message4   |Tamba Hali|

~~~
