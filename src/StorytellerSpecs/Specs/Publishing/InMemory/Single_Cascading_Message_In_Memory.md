# Single Cascading Message In Memory

-> id = 09f016d0-3a1f-4e59-96ec-fa1cc5f9ed32
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-04-18T21:35:24.3173393Z
-> tags =

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1
    ``` channel
    loopback://one
    ```

    |> SendMessage messageType=Message2
    ``` channel
    loopback://two
    ```

    |> ReceivingMessage1CascadesMessage2
    |> ListenForMessagesFrom
    ``` channel
    loopback://two
    ```


|> SendMessage messageType=Message1, name=Jamaal Charles
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt  |MessageType|Name          |
    |loopback://one|Message1   |Jamaal Charles|
    |loopback://two|Message2   |Jamaal Charles|

~~~
