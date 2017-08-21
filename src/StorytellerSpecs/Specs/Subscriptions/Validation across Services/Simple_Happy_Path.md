# Simple Happy Path

-> id = 25203f15-d72d-4b5d-950f-df36da4dab1e
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-08-18T20:23:51.4418390Z
-> tags = 

[Capabilities]
|> ForService
    [ServiceCapability]
    |> ServiceNameIs serviceName=Publisher
    |> Publishes messageType=Message1
    |> Publishes messageType=Message2
    |> Publishes messageType=Message3

|> ForService
    [ServiceCapability]
    |> ServiceNameIs serviceName=Receiver
    |> HandlesMessages
        [table]
        |MessageType|
        |Message1   |
        |Message2   |
        |Message3   |

    |> DefaultSubscriptionReceiverIs uri=loopback://one/

|> ValidationShouldBe
    [MessagingGraph]
    |> NoSubscriptionErrors
    |> TheMessageTracksShouldBe
        [rows]
        |MessageType|Publisher|Receiver|ContentType     |
        |Message1   |Publisher|Receiver|application/json|
        |Message2   |Publisher|Receiver|application/json|
        |Message3   |Publisher|Receiver|application/json|


~~~
