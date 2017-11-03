# Multiple Cascading Messages with LQ

-> id = 75de41e9-02d0-41d0-be5f-df8d67f1a765
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-11-03T19:20:03.6605190Z
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

    |> SendMessage messageType=Message4
    ``` channel
    durable://localhost:2201/four
    ```

    |> ReceivingMessage2CascadesMultiples

|> SendMessage messageType=Message2, name=Tamba Hali
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt          |MessageType|Name      |
    |tcp://localhost:2201|Message2   |Tamba Hali|
    |tcp://localhost:2201|Message3   |Tamba Hali|
    |tcp://localhost:2201|Message4   |Tamba Hali|

~~~
