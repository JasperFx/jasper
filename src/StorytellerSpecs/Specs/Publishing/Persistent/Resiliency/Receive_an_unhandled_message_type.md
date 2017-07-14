# Receive an unhandled message type

-> id = 9387a12d-5d59-44dc-9add-02c51c49b29e
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-04-06T15:00:58.4857118Z
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

|> SendMessageDirectly messageType=UnhandledMessage, name=Bill
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
