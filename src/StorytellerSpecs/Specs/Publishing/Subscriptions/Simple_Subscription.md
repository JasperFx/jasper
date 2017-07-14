# Simple subscription

-> id = c3ce4086-1b86-413f-990d-68cbb82d9ae7
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-07-14T15:13:29.6099650Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SubscribeLocally messageType=Message1
    ``` channel
    lq.tcp://localhost:2201/one
    ```

    |> ListenForMessagesFrom
    ``` channel
    lq.tcp://localhost:2201/four
    ```

    |> ListenForMessagesFrom
    ``` channel
    lq.tcp://localhost:2201/one
    ```


|> SendMessage messageType=Message1, name=James
|> TheMessagesSentShouldBe
    [rows]
    |> TheMessagesSentShouldBe-row MessageType=Message1, Name=James
    ``` ReceivedAt
    lq.tcp://localhost:2201/four
    ```


~~~
