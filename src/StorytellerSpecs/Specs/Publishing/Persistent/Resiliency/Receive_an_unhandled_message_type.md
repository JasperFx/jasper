# Receive an unhandled message type

-> id = 9387a12d-5d59-44dc-9add-02c51c49b29e
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-10-19T18:29:31.8479670Z
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

|> SendMessageDirectly messageType=UnhandledMessage, name=Bill
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
