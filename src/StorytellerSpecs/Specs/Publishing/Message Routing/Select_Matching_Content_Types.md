# Select Matching Content Types

-> id = 9f245451-fab4-4004-bf35-43dcc047312c
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-07-13T16:29:35.4045020Z
-> tags =

[BusRouting]
|> SubscriptionsAre
    [table]
    |MessageType|Destination                |Accepts                |
    |Message1   |loopback://one/              |blue,green,red         |
    |Message1   |loopback://two/              |purple,application/json|
    |Message1   |loopback://three/            |NULL                   |
    |Message1   |loopback://four/             |missing,green          |
    |Message1   |jasper://localhost:2201/one|red, purple            |

|> SerializersAre contentTypes=green
|> CustomWritersAre
    [table]
    |> CustomWritersAre-row MessageType=Message1, ContentType=blue


There are no matching content types for the subscription to jasper://localhost:2201/one, so it doesn't appear here

|> ForMessage MessageType=Message1
|> TheRoutesShouldBe
    [rows]
    |Destination   |ContentType     |
    |loopback://one  |blue            |
    |loopback://two  |application/json|
    |loopback://three|application/json|
    |loopback://four |green           |

~~~
