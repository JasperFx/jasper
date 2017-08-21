# Mismatched Transports

-> id = 784ec495-bbbd-40ea-854f-f9b1114c538a
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-08-21T15:26:37.9255080Z
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

    |> SubscribeToAllHandledMessages destination=fake://one
    |> AdditionalTransportsAre schemes=fake

|> ValidationShouldBe
    [MessagingGraph]
    |> MismatchesAre
        [rows]
        |MessageType|Publisher|Subscriber|
        |Message1   |Publisher|Receiver  |
        |Message2   |Publisher|Receiver  |
        |Message3   |Publisher|Receiver  |

    |> ForMismatch messageType=Message2, publisher=Publisher, subscriber=Receiver
    |> MismatchPropertiesAre IncompatibleTransports=True, IncompatibleContentTypes=False

~~~
