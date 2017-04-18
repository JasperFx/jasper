# Single Cascading Message In Memory

-> id = 09f016d0-3a1f-4e59-96ec-fa1cc5f9ed32
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-04-18T20:26:39.3616162Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1
    ``` channel
    memory://localhost:2201/one
    ```

    |> SendMessage messageType=Message2
    ``` channel
    memory://localhost:2201/two
    ```

    |> ReceivingMessage1CascadesMessage2
    |> ListenForMessagesFrom
    ``` channel
    memory://localhost:2201/two
    ```


|> SendMessage messageType=Message1, name=Jamaal Charles
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt                 |MessageType|Name          |
    |memory://localhost:2201/one|Message1   |Jamaal Charles|
    |memory://localhost:2201/two|Message2   |Jamaal Charles|

~~~
