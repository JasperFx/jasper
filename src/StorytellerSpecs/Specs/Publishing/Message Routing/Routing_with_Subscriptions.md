# Routing with Subscriptions

-> id = 33359e5a-af12-480e-ac2c-af11e41571ee
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2018-08-06T00:52:32.6726160Z
-> tags = 

[BusRouting]
|> SubscriptionsAre
    [table]
    |MessageType|Destination      |Accepts         |
    |Message1   |loopback://one/  |application/json|
    |Message2   |loopback://two/  |NULL            |
    |Message1   |loopback://three/|application/json|
    |Message2   |loopback://four/ |application/json|

|> ForMessage MessageType=Message1
|> TheRoutesShouldBe
    [rows]
    |Destination     |ContentType     |
    |loopback://one  |application/json|
    |loopback://three|application/json|

|> ForMessage MessageType=Message2
|> TheRoutesShouldBe
    [rows]
    |Destination    |ContentType     |
    |loopback://two |application/json|
    |loopback://four|application/json|

~~~
