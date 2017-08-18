# Determine Capabilities with Custom Readers and Writers

-> id = 0b70c61c-01f6-44c5-af3d-0499ca2f2068
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-08-18T14:34:24.3945760Z
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

    |> Publishes messageType=Message1
    |> PublishesWithExtraContentTypes messageType=, contentTypes=fake/two, MessageType=Message2
    |> PublishesWithExtraContentTypes messageType=, MessageType=Message3
    ``` contentTypes
    fake/three, text/three
    ```

    |> DefaultSubscriptionReceiverIs receiver=
    ``` uri
    jasper://server:2000/incoming
    ```

    |> SubscribesTo messageType=Message4
    |> SubscribesTo messageType=Message5
    |> CustomReadersAre
        [table]
        |> CustomReadersAre-row messageType=Message4
        ``` contentTypes
        fake/four, text/xml
        ```



|> NoErrorsWereFound
|> ThePublishedMessagesAre
    [rows]
    |MessageType|ContentTypes                            |
    |Message1   |application/json                        |
    |Message2   |application/json, fake/two              |
    |Message3   |application/json, fake/three, text/three|

|> TheSubscriptionsAre
    [rows]
    |MessageType|Destination                  |Accept                               |
    |Message4   |jasper://server:2000/incoming|application/json, fake/four, text/xml|
    |Message5   |jasper://server:2000/incoming|application/json                     |

~~~
