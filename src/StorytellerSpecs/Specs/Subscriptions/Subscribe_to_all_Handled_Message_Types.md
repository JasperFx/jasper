# Subscribe to all Handled Message Types

-> id = af234277-481e-4dff-86a5-22ae8f615bc1
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-08-18T14:55:38.0159710Z
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

    |> SubscribeToAllHandledMessages
    ``` destination
    jasper://server1:2222/incoming
    ```


|> NoErrorsWereFound
|> TheSubscriptionsAre
    [rows]
    |MessageType|Destination                   |Accept          |
    |Message1   |jasper://server1:2222/incoming|application/json|
    |Message2   |jasper://server1:2222/incoming|application/json|
    |Message3   |jasper://server1:2222/incoming|application/json|

~~~
