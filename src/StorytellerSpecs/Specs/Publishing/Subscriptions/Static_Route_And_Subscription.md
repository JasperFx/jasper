# Static route and subscription

-> id = d1caf0df-85f5-4a19-9494-4c2c9b255030
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-05-08T18:01:29.9968539Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> ListenForMessagesFrom
    ``` channel
    lq.tcp://localhost:2201/one
    ```

    |> ListenForMessagesFrom
    ``` channel
    lq.tcp://localhost:2201/four
    ```

    |> SendMessage messageType=Message1
    ``` channel
    lq.tcp://localhost:2201/three
    ```

    |> SubscribeAt messageType=Message1
    ``` channel
    lq.tcp://localhost:2201/one
    ```

    ``` receiveChannel
    lq.tcp://localhost:2201/four
    ```


|> SendMessage messageType=Message1, name=James
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt                   |MessageType|Name |
    |lq.tcp://localhost:2201/four |Message1   |James|
    |lq.tcp://localhost:2201/three|Message1   |James|

~~~
