# Complex Happy Path Copy

-> id = e75fff9e-b804-468c-b4b8-0179f6bd8884
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-08-18T20:22:54.2569900Z
-> tags = 

[Capabilities]
|> ForService
    [ServiceCapability]
    |> ServiceNameIs serviceName=Publisher1
    |> Publishes messageType=Message1
    |> Publishes messageType=Message2
    |> Publishes messageType=Message3

|> ForService
    [ServiceCapability]
    |> ServiceNameIs serviceName=Publisher2
    |> Publishes messageType=Message4
    |> Publishes messageType=Message6

|> ForService
    [ServiceCapability]
    |> ServiceNameIs serviceName=Receiver1
    |> HandlesMessages
        [table]
        |MessageType|
        |Message1   |
        |Message2   |
        |Message3   |
        |Message6   |

    |> DefaultSubscriptionReceiverIs uri=loopback://one/
    |> SubscribesTo messageType=Message1
    |> SubscribesTo messageType=Message3
    |> SubscribesTo messageType=Message6

|> ForService
    [ServiceCapability]
    |> ServiceNameIs serviceName=SendAndReceiver
    |> HandlesMessages
        [table]
        |MessageType|
        |Message2   |
        |Message4   |
        |Message5   |

    |> Publishes messageType=Message6
    |> SubscribesTo messageType=Message2
    |> SubscribesTo messageType=Message4
    |> DefaultSubscriptionReceiverIs uri=loopback://two/

|> ValidationShouldBe
    [MessagingGraph]
    |> NoSubscriptionErrors
    |> TheMessageTracksShouldBe
        [rows]
        |MessageType|Publisher      |Receiver       |ContentType     |
        |Message1   |Publisher1     |Receiver1      |application/json|
        |Message2   |Publisher1     |SendAndReceiver|application/json|
        |Message3   |Publisher1     |Receiver1      |application/json|
        |Message4   |Publisher2     |SendAndReceiver|application/json|
        |Message6   |SendAndReceiver|Receiver1      |application/json|


~~~
