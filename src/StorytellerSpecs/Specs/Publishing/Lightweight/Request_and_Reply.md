# Request and Reply

-> id = dd5ba19b-4226-44d0-8c32-c96b90ef6ea2
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-06-29T19:26:25.6184681Z
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

The 'stub://replies' is unfortunately hard-coded into the Storyteller fixture and just denotes that yep, we were able to get the matching response

|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt                 |MessageType|Name                  |
    |jasper://localhost:2201/one|Message1   |Thomas the Tank Engine|
    |stub://replies             |Message2   |Thomas the Tank Engine|

~~~
