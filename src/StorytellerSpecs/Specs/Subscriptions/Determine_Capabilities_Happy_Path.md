# Determine Capabilities Happy Path

-> id = 41bd4c2d-d619-479e-a15f-937450c704fa
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-08-18T14:44:31.7119880Z
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

    |> Publishes messageType=Message1
    |> Publishes messageType=Message2
    |> Publishes messageType=Message3
    |> DefaultSubscriptionReceiverIs receiver=
    ``` uri
    jasper://server:2000/incoming
    ```

    |> SubscribesTo messageType=Message4
    |> SubscribesTo messageType=Message5

|> NoErrorsWereFound
|> ThePublishedMessagesAre
    [rows]
    |MessageType|ContentTypes    |
    |Message1   |application/json|
    |Message2   |application/json|
    |Message3   |application/json|

|> TheSubscriptionsAre
    [rows]
    |MessageType|Destination                  |Accept          |
    |Message4   |jasper://server:2000/incoming|application/json|
    |Message5   |jasper://server:2000/incoming|application/json|

~~~
