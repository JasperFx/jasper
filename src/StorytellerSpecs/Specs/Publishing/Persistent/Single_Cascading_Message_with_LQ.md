# Single Cascading Message with LQ

-> id = 46a7f66e-ff6a-4fbe-a1c8-9fb66b384511
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-10-19T18:27:22.7267170Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1
    ``` channel
    durable://localhost:2201/one
    ```

    |> SendMessage messageType=Message2
    ``` channel
    durable://localhost:2201/two
    ```

    |> ReceivingMessage1CascadesMessage2
    |> ListenForMessagesFrom
    ``` channel
    durable://localhost:2201/two
    ```


|> SendMessage messageType=Message1, name=Jamaal Charles
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt              |MessageType|Name          |
    |durable://localhost:2201|Message1   |Jamaal Charles|
    |durable://localhost:2201|Message2   |Jamaal Charles|

~~~
