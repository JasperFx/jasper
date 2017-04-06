# Receive a garbled message that blows up in deserialization

-> id = 2421708e-e999-4149-a3d7-91e642e85d1f
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-04-06T14:57:49.0615110Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> ListenForMessagesFrom
    ``` channel
    lq.tcp://localhost:2201/one
    ```

    |> SendMessage messageType=Message1
    ``` channel
    lq.tcp://localhost:2201/one
    ```

    |> SendMessage messageType=Message2
    ``` channel
    lq.tcp://localhost:2201/one
    ```



There is no handler for UnhandledMessage in this configuration

|> SendGarbledMessage
``` address
lq.tcp://localhost:2201/one
```

|> SendMessage messageType=Message1, name=Suzy
|> SendMessage messageType=Message2, name=Russell
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt                 |MessageType|Name   |
    |lq.tcp://localhost:2201/one|Message1   |Suzy   |
    |lq.tcp://localhost:2201/one|Message2   |Russell|

~~~
