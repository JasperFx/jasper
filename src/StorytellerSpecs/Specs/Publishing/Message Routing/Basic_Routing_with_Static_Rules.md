# Basic Routing with Static Rules

-> id = fb7cbe02-8f59-42bf-b111-a03502564db2
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-07-13T16:24:47.4114997Z
-> tags =

[BusRouting]

Nothing happening here but the default Json serializer and static publishing rules

|> SendMessage messageType=Message1, channel=loopback://three/
|> SendMessage messageType=Message1, channel=loopback://one/
|> SendMessage messageType=Message2, channel=loopback://four/
|> ForMessage MessageType=Message1
|> TheRoutesShouldBe
    [rows]
    |Destination   |ContentType     |
    |loopback://one  |application/json|
    |loopback://three|application/json|

|> ForMessage MessageType=Message2
|> TheRoutesShouldBe
    [rows]
    |> TheRoutesShouldBe-row Destination=loopback://four, ContentType=application/json

~~~
