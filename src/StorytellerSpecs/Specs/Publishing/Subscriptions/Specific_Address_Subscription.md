# Specific address subscription

-> id = 03a13d21-ffa2-476f-bf10-9c9905e4dc31
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-05-08T18:02:10.5298539Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> ListenForMessagesFrom
    ``` channel
    lq.tcp://localhost:2201/one
    ```

    |> SubscribeAt messageType=Message1
    ``` channel
    lq.tcp://localhost:2201/one
    ```

    ``` receiveChannel
    lq.tcp://localhost:2201/three
    ```

    |> ListenForMessagesFrom
    ``` channel
    lq.tcp://localhost:2201/four
    ```

    |> ListenForMessagesFrom
    ``` channel
    lq.tcp://localhost:2201/three
    ```


|> SendMessage messageType=Message1, name=James
|> TheMessagesSentShouldBe
    [rows]
    |> TheMessagesSentShouldBe-row MessageType=Message1, Name=James
    ``` ReceivedAt
    lq.tcp://localhost:2201/three
    ```


~~~
