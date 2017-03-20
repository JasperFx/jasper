# Simplest Possible Happy Path

-> id = 63d1dfc5-e1bc-49fc-8a88-5d846f847c6d
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-03-14T18:40:57.8082814Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1, channel=stub://one
    |> SendMessage messageType=Message2, channel=stub://two
    |> SendMessage messageType=Message3, channel=stub://three

|> SendMessage messageType=Message1, name=Tom
|> SendMessage messageType=Message2, name=Todd
|> SendMessage messageType=Message3, name=Trevor
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt  |MessageType|Name  |
    |stub://one  |Message1   |Tom   |
    |stub://two  |Message2   |Todd  |
    |stub://three|Message3   |Trevor|

~~~
