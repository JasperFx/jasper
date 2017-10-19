# Request & Reply Mechanics

-> id = 1b98469a-7960-449f-8879-ca1aef81680d
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-10-19T14:55:07.3407840Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> ReceivingMessage1CascadesMessage2
    |> SendMessage messageType=Message1, channel=loopback://one/

|> RequestAndReply name=Thomas the Tank Engine
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt    |MessageType|Name                  |
    |loopback://one|Message1   |Thomas the Tank Engine|
    |stub://replies|Message2   |Thomas the Tank Engine|

~~~
