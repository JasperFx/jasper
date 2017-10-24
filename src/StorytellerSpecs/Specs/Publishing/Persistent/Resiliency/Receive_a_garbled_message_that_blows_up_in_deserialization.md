# Receive a garbled message that blows up in deserialization

-> id = 2421708e-e999-4149-a3d7-91e642e85d1f
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-10-19T18:29:00.2994410Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> ListenForMessagesFrom
    ``` channel
    durable://localhost:2201/one
    ```

    |> SendMessage messageType=Message1
    ``` channel
    durable://localhost:2201/one
    ```

    |> SendMessage messageType=Message2
    ``` channel
    durable://localhost:2201/one
    ```



There is no handler for UnhandledMessage in this configuration

|> SendGarbledMessage
``` address
durable://localhost:2201/one
```

|> SendMessage messageType=Message1, name=Suzy
|> SendMessage messageType=Message2, name=Russell
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt              |MessageType|Name   |
    |durable://localhost:2201|Message1   |Suzy   |
    |durable://localhost:2201|Message2   |Russell|

~~~
