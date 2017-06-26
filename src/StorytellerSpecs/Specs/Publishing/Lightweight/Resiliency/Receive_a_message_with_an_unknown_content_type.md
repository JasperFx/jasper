# Receive a message with an unknown content type

-> id = b9cffe0d-2b61-48eb-b5fd-7b1f5eec522a
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-06-26T19:52:19.8307215Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> ListenForMessagesFrom
    ``` channel
    jasper://localhost:2201/one
    ```

    |> SendMessage messageType=Message1
    ``` channel
    jasper://localhost:2201/one
    ```

    |> SendMessage messageType=Message2
    ``` channel
    jasper://localhost:2201/one
    ```


|> SendMessageWithUnknownContentType
``` address
jasper://localhost:2201/one
```

|> SendMessage messageType=Message1, name=Suzy
|> SendMessage messageType=Message2, name=Russell
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt                 |MessageType|Name   |
    |jasper://localhost:2201/one|Message1   |Suzy   |
    |jasper://localhost:2201/one|Message2   |Russell|

~~~
