# Mix of Subscriptions and Static Rules

-> id = 3221f1cc-59d0-4cfb-93cb-b42c8d47eb8c
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-07-13T16:29:42.4676280Z
-> tags =

[BusRouting]
|> SendMessage messageType=Message1, channel=loopback://one/
|> SendMessage messageType=Message2
``` channel
tcp://localhost:2201/one
```

|> SubscriptionsAre
    [table]
    |MessageType|Destination    |Accepts         |
    |Message1   |loopback://two/  |application/json|
    |Message1   |loopback://three/|NULL            |
    |Message2   |loopback://four/ |application/json|

|> ForMessage MessageType=Message1
|> TheRoutesShouldBe
    [rows]
    |Destination   |ContentType     |
    |loopback://one  |application/json|
    |loopback://two  |application/json|
    |loopback://three|application/json|

|> ForMessage MessageType=Message2
~~~
