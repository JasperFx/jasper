# Simple subscription

-> id = c3ce4086-1b86-413f-990d-68cbb82d9ae7
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-07-14T20:13:03.5710174Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SubscribeLocally messageType=Message1
    ``` channel
    jasper://localhost:2201/one
    ```

    |> ListenForMessagesFrom
    ``` channel
    jasper://localhost:2201/four
    ```

    |> ListenForMessagesFrom
    ``` channel
    jasper://localhost:2201/one
    ```


|> SendMessage messageType=Message1, name=James
|> TheMessagesSentShouldBe
    [rows]
    |> TheMessagesSentShouldBe-row MessageType=Message1, Name=James
    ``` ReceivedAt
    jasper://localhost:2201/four
    ```


~~~
