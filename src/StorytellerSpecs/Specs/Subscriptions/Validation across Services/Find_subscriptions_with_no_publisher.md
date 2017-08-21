# Find subscriptions with no publisher

-> id = cc99ca65-7889-4aae-8ea5-b1510511c95a
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-08-21T15:17:16.3122200Z
-> tags = 

[Capabilities]
|> ForService
    [ServiceCapability]
    |> ServiceNameIs serviceName=Publisher1
    |> Publishes messageType=Message3

|> ForService
    [ServiceCapability]
    |> ServiceNameIs serviceName=Publisher2
    |> Publishes messageType=Message4

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
    |> TheMessageTracksShouldBe
        [rows]
        |MessageType|Publisher      |Receiver       |ContentType     |
        |Message3   |Publisher1     |Receiver1      |application/json|
        |Message4   |Publisher2     |SendAndReceiver|application/json|
        |Message6   |SendAndReceiver|Receiver1      |application/json|

    |> NoPublishersShouldBe
        [rows]
        |ServiceName    |MessageType|Destination   |
        |Receiver1      |Message1   |loopback://one|
        |SendAndReceiver|Message2   |loopback://two|


~~~
