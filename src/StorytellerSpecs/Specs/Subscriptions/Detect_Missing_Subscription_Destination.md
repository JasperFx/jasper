# Detect Missing Subscription Destination

-> id = 366a8304-6300-4138-94b7-dfb6f8dd6d65
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-08-18T14:50:38.8049780Z
-> tags = 

[Capabilities]
|> ForService
    [ServiceCapability]
    |> HandlesMessages
        [table]
        |MessageType|
        |Message4   |
        |Message5   |

    |> SubscribesTo messageType=Message4
    |> SubscribesAtLocation messageType=Message5, receiver=loopback://one/

|> TheSubscriptionsAre
    [rows]
    |> TheSubscriptionsAre-row MessageType=Message5, Destination=loopback://one, Accept=application/json

|> TheErrorsDetectedWere
    [Rows]
    |> TheErrorsDetectedWere-row
    ``` expected
    Could not determine an incoming receiver for message "Message4"
    ```


~~~
