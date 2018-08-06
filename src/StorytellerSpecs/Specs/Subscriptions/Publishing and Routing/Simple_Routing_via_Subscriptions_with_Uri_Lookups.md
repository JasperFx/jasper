# Simple Routing via Subscriptions with Uri Lookups

-> id = 4e42cba1-71a8-4985-8e1f-cd415ad5f754
-> lifecycle = Regression
-> max-retries = 3
-> last-updated = 2018-08-06T00:52:38.0044960Z
-> tags = 

[Communication]
|> UriAliasesAre
    [table]
    |Alias        |Actual                       |
    |standin://one|tcp://localhost:2244/incoming|
    |standin://two|tcp://localhost:2255/incoming|

|> ForService
    [Node]
    |> ForService serviceName=Receiver1
    |> SubscribeAtUri messageType=Message3, uri=standin://one

|> ForService
    [Node]
    |> ForService serviceName=Receiver2
    |> SubscribeAtUri messageType=Message3, uri=standin://two
    |> SubscribeAtUri messageType=Message4, uri=standin://two

|> SendMessage messageType=Message3, name=Bill
|> SendMessage messageType=Message3, name=Tom
|> SendMessage messageType=Message4, name=George
|> TheMessagesSentShouldBe
    [rows]
    |ServiceName|MessageType|Name  |
    |Receiver1  |Message3   |Bill  |
    |Receiver2  |Message3   |Bill  |
    |Receiver1  |Message3   |Tom   |
    |Receiver2  |Message3   |Tom   |
    |Receiver2  |Message4   |George|

~~~
