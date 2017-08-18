# Determine Capabilities with Explicit Destinations

-> id = e4884438-3800-4bc0-af31-f8d53fddb848
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-08-18T19:49:03.6642880Z
-> tags = 

[Capabilities]
|> ForService
    [ServiceCapability]
    |> HandlesMessages
        [table]
        |MessageType|
        |Message1   |
        |Message2   |
        |Message3   |
        |Message4   |
        |Message5   |

    |> DefaultSubscriptionReceiverIs
    ``` uri
    jasper://server:2000/incoming
    ```

    |> SubscribesAtLocation messageType=Message4
    ``` receiver
    jasper://localhost:2201/one
    ```

    |> SubscribesTo messageType=Message5

|> NoErrorsWereFound
|> TheSubscriptionsAre
    [rows]
    |MessageType|Destination                  |Accept          |
    |Message4   |jasper://localhost:2201/one  |application/json|
    |Message5   |jasper://server:2000/incoming|application/json|

~~~
