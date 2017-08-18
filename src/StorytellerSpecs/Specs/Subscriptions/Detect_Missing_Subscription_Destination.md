# Detect Missing Subscription Destination

-> id = 366a8304-6300-4138-94b7-dfb6f8dd6d65
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-08-18T19:49:03.6677320Z
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
    |MessageType|Destination   |Accept          |
    |Message5   |loopback://one|application/json|
    |Message4   |NULL          |application/json|

|> TheErrorsDetectedWere
    [Rows]
    |> TheErrorsDetectedWere-row
    ``` expected
    Could not determine an incoming receiver for message 'Message4'
    ```


~~~
