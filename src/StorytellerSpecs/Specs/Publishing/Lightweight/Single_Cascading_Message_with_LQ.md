# Single Cascading Message with LQ

-> id = 46a7f66e-ff6a-4fbe-a1c8-9fb66b384510
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-06-26T19:52:19.8377215Z
-> tags =

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1
    ``` channel
    tcp://localhost:2201/one
    ```

    |> SendMessage messageType=Message2
    ``` channel
    tcp://localhost:2201/two
    ```

    |> ReceivingMessage1CascadesMessage2
    |> ListenForMessagesFrom
    ``` channel
    tcp://localhost:2201/two
    ```


|> SendMessage messageType=Message1, name=Jamaal Charles
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt                 |MessageType|Name          |
    |tcp://localhost:2201/one|Message1   |Jamaal Charles|
    |tcp://localhost:2201/two|Message2   |Jamaal Charles|

~~~
