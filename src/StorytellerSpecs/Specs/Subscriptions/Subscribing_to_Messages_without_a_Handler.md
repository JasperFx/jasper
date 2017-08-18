# Subscribing to Messages without a Handler

-> id = 4c5b692a-c0a0-4478-8ffa-b3c8c66434e8
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-08-18T14:48:09.6481490Z
-> tags = 

[Capabilities]
|> ForService
    [ServiceCapability]
    |> HandlesMessages
        [table]
        |> HandlesMessages-row MessageType=Message4

    |> DefaultSubscriptionReceiverIs receiver=
    ``` uri
    jasper://server:2000/incoming
    ```

    |> SubscribesTo messageType=Message4
    |> SubscribesTo messageType=Message5

|> TheSubscriptionsAre
    [rows]
    |MessageType|Destination                  |Accept          |
    |Message4   |jasper://server:2000/incoming|application/json|
    |Message5   |jasper://server:2000/incoming|application/json|

|> TheErrorsDetectedWere
    [Rows]
    |> TheErrorsDetectedWere-row
    ``` expected
    No handler for message "Message5" referenced in a subscription
    ```


~~~
