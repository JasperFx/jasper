# Send to a Specific Channel

-> id = 0259b104-8871-4616-891d-50d7f3a046a9
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-03-14T19:59:53.7519650Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1, channel=stub://one
    |> SendMessage messageType=Message2, channel=stub://two
    |> SendMessage messageType=Message3, channel=stub://three
    |> SendMessage messageType=Message1, channel=stub://four
    |> SendMessage messageType=Message2, channel=stub://four

|> SendMessageDirectly messageType=Message1, name=Hank, address=stub://three
|> TheMessagesSentShouldBe
    [rows]
    |> TheMessagesSentShouldBe-row ReceivedAt=stub://three, MessageType=Message1, Name=Hank

~~~
