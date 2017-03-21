# Single Cascading Message

-> id = cc98565d-c1b9-4c2b-8902-53a5ad78f62e
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-03-21T15:08:42.6777580Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1, channel=stub://one
    |> SendMessage messageType=Message2, channel=stub://two
    |> ReceivingMessage1CascadesMessage2

|> SendMessage messageType=Message1, name=Jamaal Charles
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt|MessageType|Name          |
    |stub://one|Message1   |Jamaal Charles|
    |stub://two|Message2   |Jamaal Charles|

~~~
