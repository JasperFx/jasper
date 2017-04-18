# Messages Sent to Multiple Channels

-> id = c553e31b-702b-4c38-991b-4eb2c28f2424
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-03-14T19:57:53.9940780Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1, channel=stub://one
    |> SendMessage messageType=Message2, channel=stub://two
    |> SendMessage messageType=Message3, channel=stub://three
    |> SendMessage messageType=Message1, channel=stub://four
    |> SendMessage messageType=Message2, channel=stub://four

|> SendMessage messageType=Message1, name=Tom
|> SendMessage messageType=Message2, name=Todd
|> SendMessage messageType=Message3, name=Trevor
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt  |MessageType|Name  |
    |stub://one  |Message1   |Tom   |
    |stub://two  |Message2   |Todd  |
    |stub://three|Message3   |Trevor|
    |stub://four |Message2   |Todd  |
    |stub://four |Message1   |Tom   |

~~~
