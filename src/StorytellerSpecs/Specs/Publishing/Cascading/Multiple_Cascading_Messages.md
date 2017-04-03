# Multiple Cascading Messages

-> id = aadf27bf-da7b-4ae0-a316-a51e5bdc7787
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-04-03T12:44:47.0403399Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1, channel=stub://one
    |> SendMessage messageType=Message2, channel=stub://two
    |> SendMessage messageType=Message3, channel=stub://three
    |> SendMessage messageType=Message4, channel=stub://four
    |> ReceivingMessage2CascadesMultiples

|> SendMessage messageType=Message2, name=Tamba Hali
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt  |MessageType|Name      |
    |stub://two  |Message2   |Tamba Hali|
    |stub://three|Message3   |Tamba Hali|
    |stub://four |Message4   |Tamba Hali|

~~~
