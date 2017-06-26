# Single Cascading Message with LQ

-> id = 46a7f66e-ff6a-4fbe-a1c8-9fb66b384510
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-06-26T14:29:52.0064940Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1
    ``` channel
    jasper://localhost:2201/one
    ```

    |> SendMessage messageType=Message2
    ``` channel
    jasper://localhost:2201/two
    ```

    |> ReceivingMessage1CascadesMessage2
    |> ListenForMessagesFrom
    ``` channel
    jasper://localhost:2201/two
    ```


|> SendMessage messageType=Message1, name=Jamaal Charles
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt                 |MessageType|Name          |
    |jasper://localhost:2201/one|Message1   |Jamaal Charles|
    |jasper://localhost:2201/two|Message2   |Jamaal Charles|

~~~
