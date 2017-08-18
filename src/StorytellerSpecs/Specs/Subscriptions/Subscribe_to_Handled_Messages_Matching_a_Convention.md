# Subscribe to Handled Messages Matching a Convention

-> id = bc2e9048-ca01-4cae-9788-31299fda3879
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-08-18T15:06:04.4869860Z
-> tags = 

[Capabilities]
|> ForService
    [ServiceCapability]
    |> HandlesMessages
        [table]
        |MessageType     |
        |Message1        |
        |Message2        |
        |Message3        |
        |ErrorMessage    |
        |UnhandledMessage|

    |> DefaultSubscriptionReceiverIs receiver=
    ``` uri
    jasper://server1:2222/incoming
    ```

    |> SubscribeToAllMessagesStartingWithM

|> NoErrorsWereFound
|> TheSubscriptionsAre
    [rows]
    |MessageType|Destination                   |Accept          |
    |Message1   |jasper://server1:2222/incoming|application/json|
    |Message2   |jasper://server1:2222/incoming|application/json|
    |Message3   |jasper://server1:2222/incoming|application/json|

~~~
