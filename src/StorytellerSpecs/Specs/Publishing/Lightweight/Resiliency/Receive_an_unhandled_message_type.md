# Receive an unhandled message type

-> id = 9387a12d-5d59-44dc-9add-02c51c49b29d
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-06-26T19:52:19.8157215Z
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



There is no handler for UnhandledMessage in this configuration

|> SendMessageDirectly messageType=UnhandledMessage, name=Bill
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
