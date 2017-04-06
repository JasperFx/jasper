# Receive a message with an unknown content type

-> id = b9cffe0d-2b61-48eb-b5fd-7b1f5eec522b
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-04-06T15:00:48.0606694Z
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


|> SendMessageWithUnknownContentType
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
