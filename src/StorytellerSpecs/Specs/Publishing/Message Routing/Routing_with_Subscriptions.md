# Routing with Subscriptions

-> id = 33359e5a-af12-480e-ac2c-af11e41571ee
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-07-13T14:39:46.4334010Z
-> tags = 

[BusRouting]
|> SubscriptionsAre
    [table]
    |MessageType|Destination    |Accepts         |
    |Message1   |memory://one/  |application/json|
    |Message2   |memory://two/  |NULL            |
    |Message1   |memory://three/|application/json|
    |Message2   |memory://four/ |application/json|

|> ForMessage MessageType=Message1
|> TheRoutesShouldBe
    [rows]
    |Destination   |ContentType     |
    |memory://one  |application/json|
    |memory://three|application/json|

|> ForMessage MessageType=Message2
|> TheRoutesShouldBe
    [rows]
    |Destination  |ContentType     |
    |memory://two |application/json|
    |memory://four|application/json|

~~~
