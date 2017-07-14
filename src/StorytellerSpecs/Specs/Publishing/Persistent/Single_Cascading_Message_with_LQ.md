# Single Cascading Message with LQ

-> id = 46a7f66e-ff6a-4fbe-a1c8-9fb66b384511
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-06-26T14:29:52.0144240Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1
    ``` channel
    lq.tcp://localhost:2201/one
    ```

    |> SendMessage messageType=Message2
    ``` channel
    lq.tcp://localhost:2201/two
    ```

    |> ReceivingMessage1CascadesMessage2
    |> ListenForMessagesFrom
    ``` channel
    lq.tcp://localhost:2201/two
    ```


|> SendMessage messageType=Message1, name=Jamaal Charles
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt                 |MessageType|Name          |
    |lq.tcp://localhost:2201/one|Message1   |Jamaal Charles|
    |lq.tcp://localhost:2201/two|Message2   |Jamaal Charles|

~~~
