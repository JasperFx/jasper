# Mix of Subscriptions and Static Rules

-> id = 3221f1cc-59d0-4cfb-93cb-b42c8d47eb8c
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-07-13T16:29:42.4676280Z
-> tags = 

[BusRouting]
|> SendMessage messageType=Message1, channel=memory://one/
|> SendMessage messageType=Message2
``` channel
jasper://localhost:2201/one
```

|> SubscriptionsAre
    [table]
    |MessageType|Destination    |Accepts         |
    |Message1   |memory://two/  |application/json|
    |Message1   |memory://three/|NULL            |
    |Message2   |memory://four/ |application/json|

|> ForMessage MessageType=Message1
|> TheRoutesShouldBe
    [rows]
    |Destination   |ContentType     |
    |memory://one  |application/json|
    |memory://two  |application/json|
    |memory://three|application/json|

|> ForMessage MessageType=Message2
~~~
