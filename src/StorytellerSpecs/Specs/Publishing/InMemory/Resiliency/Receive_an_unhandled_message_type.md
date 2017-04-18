# Receive an unhandled message type

-> id = 92c6a89e-0062-44d0-9d8f-3bb93cae0f37
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-04-18T15:00:58.4857118Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> ListenForMessagesFrom
    ``` channel
    memory://localhost:2201/one
    ```

    |> SendMessage messageType=Message1
    ``` channel
    memory://localhost:2201/one
    ```

    |> SendMessage messageType=Message2
    ``` channel
    memory://localhost:2201/one
    ```



There is no handler for UnhandledMessage in this configuration

|> SendMessageDirectly messageType=UnhandledMessage, name=Bill
``` address
memory://localhost:2201/one
```

|> SendMessage messageType=Message1, name=Suzy
|> SendMessage messageType=Message2, name=Russell
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt                 |MessageType|Name   |
    |memory://localhost:2201/one|Message1   |Suzy   |
    |memory://localhost:2201/one|Message2   |Russell|

~~~
