# Simple Routing via Subscriptions with Uri Lookups

-> id = 4e42cba1-71a8-4985-8e1f-cd415ad5f754
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-08-23T13:27:13.9783750Z
-> tags =

[Communication]
|> UriAliasesAre
    [table]
    |Alias        |Actual                          |
    |standin://one|jasper://localhost:2222/incoming|
    |standin://two|jasper://localhost:2233/incoming|

|> ForService
    [Node]
    |> ForService serviceName=Receiver1
    |> SubscribeAtUri messageType=Message1, uri=standin://one

|> ForService
    [Node]
    |> ForService serviceName=Receiver2
    |> SubscribeAtUri messageType=Message1, uri=standin://two
    |> SubscribeAtUri messageType=Message2, uri=standin://two

|> SendMessage messageType=Message1, name=Bill
|> SendMessage messageType=Message1, name=Tom
|> SendMessage messageType=Message2, name=George
|> TheMessagesSentShouldBe
    [rows]
    |ServiceName|MessageType|Name  |
    |Receiver1  |Message1   |Bill  |
    |Receiver2  |Message1   |Bill  |
    |Receiver1  |Message1   |Tom   |
    |Receiver2  |Message1   |Tom   |
    |Receiver2  |Message2   |George|

~~~
