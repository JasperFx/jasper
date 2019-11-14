# Receive a garbled message that blows up in deserialization

-> id = 41b8fa44-6b0e-40ce-8f93-8d20cafa10dd
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-04-18T14:57:49.0615110Z
-> tags =

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> ListenForMessagesFrom
    ``` channel
    local://one
    ```

    |> SendMessage messageType=Message1
    ``` channel
    local://one
    ```

    |> SendMessage messageType=Message2
    ``` channel
    local://one
    ```



There is no handler for UnhandledMessage in this configuration

|> SendGarbledMessage
``` address
local://one
```

|> SendMessage messageType=Message1, name=Suzy
|> SendMessage messageType=Message2, name=Russell
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt                 |MessageType|Name   |
    |local://one|Message1   |Suzy   |
    |local://one|Message2   |Russell|

~~~
