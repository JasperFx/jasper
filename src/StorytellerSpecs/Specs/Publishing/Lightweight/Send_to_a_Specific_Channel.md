# Send to a Specific Channel

-> id = 0259b104-8871-4616-891d-20d7f3a046a8
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-06-26T19:51:36.1617215Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1, channel=stub://one
    |> SendMessage messageType=Message2
    ``` channel
    jasper://localhost:2201/two
    ```

    |> SendMessage messageType=Message3
    ``` channel
    jasper://localhost:2201/three
    ```

    |> SendMessage messageType=Message1
    ``` channel
    jasper://localhost:2201/four
    ```

    |> SendMessage messageType=Message2
    ``` channel
    jasper://localhost:2201/four
    ```

    |> ListenForMessagesFrom
    ``` channel
    jasper://localhost:2201/three
    ```


|> SendMessageDirectly messageType=Message1, name=Hank
``` address
jasper://localhost:2201/three
```

|> TheMessagesSentShouldBe
    [rows]
    |> TheMessagesSentShouldBe-row MessageType=Message1, Name=Hank
    ``` ReceivedAt
    jasper://localhost:2201/three
    ```


~~~
