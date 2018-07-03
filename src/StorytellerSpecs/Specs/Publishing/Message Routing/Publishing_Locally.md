# Publishing Locally

-> id = 98da68d8-e605-4d91-990e-6db5e1b1910f
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2018-07-03T12:45:15.5411140Z
-> tags = 

[BusRouting]
|> PublishLocally MessageType=Message1
|> PublishLocally MessageType=Message2
|> PublishLocally MessageType=Message3
|> Handles MessageType=Message1
|> Handles MessageType=Message2
|> Handles MessageType=Message4
|> Handles MessageType=Message5
|> SendMessage messageType=Message1, channel=tcp://localhost:2201/one
|> SendMessage messageType=Message4, channel=tcp://localhost:2201/four

Message1 is handled, published locally, and has an additional subscriber

|> ForMessage MessageType=Message1
|> TheRoutesShouldBe
    [rows]
    |Destination             |ContentType     |
    |tcp://localhost:2201/one|application/json|
    |loopback://default      |application/json|


Message2 is handled and published locally

|> ForMessage MessageType=Message2
|> TheRoutesShouldBe
    [rows]
    |> TheRoutesShouldBe-row Destination=loopback://default, ContentType=application/json


Message 3 is not handled, so no routes

|> ForMessage MessageType=Message3
|> NoRoutesFor

Message 4 has a static rule, but is not published locally even though it has a handler for Message 4

|> ForMessage MessageType=Message4
|> TheRoutesShouldBe
    [rows]
    |> TheRoutesShouldBe-row Destination=tcp://localhost:2201/four, ContentType=application/json


Message 5 has a handler, but no other publishing rules, so handle it locally

|> ForMessage MessageType=Message5
|> TheRoutesShouldBe
    [rows]
    |> TheRoutesShouldBe-row Destination=loopback://default, ContentType=application/json

~~~
