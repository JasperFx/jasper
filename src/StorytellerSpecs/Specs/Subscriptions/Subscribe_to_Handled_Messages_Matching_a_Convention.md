# Subscribe to Handled Messages Matching a Convention

-> id = bc2e9048-ca01-4cae-9788-31299fda3879
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-08-18T19:49:03.6730230Z
-> tags =

[Capabilities]
|> ForService
    [ServiceCapability]
    |> HandlesMessages
        [table]
        |MessageType |
        |Message1    |
        |Message2    |
        |Message3    |
        |ErrorMessage|

    |> DefaultSubscriptionReceiverIs
    ``` uri
    tcp://server1:2222/incoming
    ```

    |> SubscribeToAllMessagesStartingWithM

|> NoErrorsWereFound
|> TheSubscriptionsAre
    [rows]
    |MessageType|Destination                   |Accept          |
    |Message1   |tcp://server1:2222/incoming|application/json|
    |Message2   |tcp://server1:2222/incoming|application/json|
    |Message3   |tcp://server1:2222/incoming|application/json|

~~~
