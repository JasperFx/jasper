# Subscribe with Invalid Transport Destination

-> id = 49ebf6b4-74c5-4a0e-b074-17eab89e9ef1
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-08-18T19:49:03.6536990Z
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

    |> DefaultSubscriptionReceiverIs uri=wrong://server:2000
    |> SubscribesTo messageType=Message4
    |> SubscribesTo messageType=Message5

|> TheSubscriptionsAre
    [rows]
    |MessageType|Destination        |Accept          |
    |Message4   |wrong://server:2000|application/json|
    |Message5   |wrong://server:2000|application/json|

|> TheErrorsDetectedWere
    [Rows]
    |expected                                                        |
    |Unknown transport 'wrong' for subscription to message 'Message4'|
    |Unknown transport 'wrong' for subscription to message 'Message5'|

~~~
