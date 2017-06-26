# Request and Reply

-> id = dd5ba19b-4226-44d0-8c32-c96b90ef6ea2
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-06-26T14:15:17.7230300Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> ReceivingMessage1CascadesMessage2
    |> SendMessage messageType=Message1
    ``` channel
    jasper://localhost:2201/one
    ```

    |> ListenForMessagesFrom
    ``` channel
    jasper://localhost:2201/one
    ```


|> RequestAndReply name=Thomas the Tank Engine
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt                     |MessageType|Name                  |
    |jasper://localhost:2201/one    |Message1   |Thomas the Tank Engine|
    |jasper://localhost:2201/replies|Message2   |Thomas the Tank Engine|

~~~
