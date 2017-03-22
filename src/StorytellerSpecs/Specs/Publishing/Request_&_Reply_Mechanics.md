# Request & Reply Mechanics

-> id = 1b98469a-7960-449f-8879-ca1aef81680d
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-03-22T13:29:25.0388371Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> ReceivingMessage1CascadesMessage2
    |> SendMessage messageType=Message1, channel=stub://one

|> RequestAndReply name=Thomas the Tank Engine
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt    |MessageType|Name                  |
    |stub://one    |Message1   |Thomas the Tank Engine|
    |stub://replies|Message2   |Thomas the Tank Engine|

~~~
