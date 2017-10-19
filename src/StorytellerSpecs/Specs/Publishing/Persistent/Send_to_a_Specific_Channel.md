# Send to a Specific Channel

-> id = 0259b104-8871-4616-891d-50d7f3a046a9
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-10-19T18:27:47.5813750Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1, channel=stub://one
    |> SendMessage messageType=Message2
    ``` channel
    durable://localhost:2201/two
    ```

    |> SendMessage messageType=Message3
    ``` channel
    durable://localhost:2201/three
    ```

    |> SendMessage messageType=Message1
    ``` channel
    durable://localhost:2201/four
    ```

    |> SendMessage messageType=Message2
    ``` channel
    durable://localhost:2201/four
    ```


|> SendMessageDirectly messageType=Message1, name=Hank
``` address
durable://localhost:2201/three
```

|> TheMessagesSentShouldBe
    [rows]
    |> TheMessagesSentShouldBe-row ReceivedAt=durable://localhost:2201, MessageType=Message1, Name=Hank

~~~
