# Publish Everything Locally

-> id = 34557434-e6fa-432d-bb92-f70aefacef37
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2018-07-03T12:50:37.7375700Z
-> tags = 

[BusRouting]
|> PublishAllLocally
|> Handles MessageType=Message1
|> Handles MessageType=Message2
|> Handles MessageType=Message3
|> SendMessage messageType=Message1, channel=tcp://localhost:2201/one
|> ForMessage MessageType=Message1
|> TheRoutesShouldBe
    [rows]
    |Destination             |ContentType     |
    |tcp://localhost:2201/one|application/json|
    |local://default      |application/json|

|> ForMessage MessageType=Message2
|> TheRoutesShouldBe
    [rows]
    |> TheRoutesShouldBe-row Destination=local://default, ContentType=application/json

|> ForMessage MessageType=Message5
|> NoRoutesFor
~~~
