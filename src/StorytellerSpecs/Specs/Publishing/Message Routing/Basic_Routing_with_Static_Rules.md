# Basic Routing with Static Rules

-> id = fb7cbe02-8f59-42bf-b111-a03502564db2
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-07-13T14:36:36.1112880Z
-> tags = 

[BusRouting]

Nothing happening here but the default Json serializer and static publishing rules

|> SendMessage messageType=Message1
``` channel
lq.tcp://localhost:2201/three
```

|> SendMessage messageType=Message1
``` channel
lq.tcp://localhost:2201/one
```

|> SendMessage messageType=Message2
``` channel
lq.tcp://localhost:2201/four
```

|> ForMessage MessageType=Message1
|> TheRoutesShouldBe
    [rows]
    |Destination                  |ContentType     |
    |lq.tcp://localhost:2201/one  |application/json|
    |lq.tcp://localhost:2201/three|application/json|

|> ForMessage MessageType=Message2
|> TheRoutesShouldBe
    [rows]
    |> TheRoutesShouldBe-row ContentType=application/json
    ``` Destination
    lq.tcp://localhost:2201/four
    ```


~~~
