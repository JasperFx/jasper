# Find publishers with no subscribers

-> id = c9b32754-2a90-4086-a975-fbc34d6b4c01
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-08-21T15:19:36.1779810Z
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
    |> SubscribesTo messageType=Message6

|> ForService
    [ServiceCapability]
    |> ServiceNameIs serviceName=SendAndReceiver
    |> HandlesMessages
        [table]
        |MessageType|
        |Message4   |
        |Message5   |

    |> Publishes messageType=Message6
    |> SubscribesTo messageType=Message4
    |> DefaultSubscriptionReceiverIs uri=loopback://two/

|> ValidationShouldBe
    [MessagingGraph]
    |> TheMessageTracksShouldBe
        [rows]
        |MessageType|Publisher      |Receiver       |ContentType     |
        |Message1   |Publisher1     |Receiver1      |application/json|
        |Message4   |Publisher2     |SendAndReceiver|application/json|
        |Message6   |SendAndReceiver|Receiver1      |application/json|

    |> NoSubscribersShouldBe
        [rows]
        |ServiceName|MessageType|ContentTypes    |
        |Publisher1 |Message2   |application/json|
        |Publisher1 |Message3   |application/json|


~~~
