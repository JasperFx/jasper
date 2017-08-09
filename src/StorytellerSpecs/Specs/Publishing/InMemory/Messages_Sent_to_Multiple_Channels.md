# Messages Sent to Multiple Channels

-> id = 052488cb-9641-44b3-9849-0e59109feec5
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-04-18T20:49:23.6930357Z
-> tags =

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1
    ``` channel
    loopback://one
    ```

    |> SendMessage messageType=Message2
    ``` channel
    loopback://two
    ```

    |> SendMessage messageType=Message3
    ``` channel
    loopback://three
    ```

    |> SendMessage messageType=Message1
    ``` channel
    loopback://four
    ```

    |> SendMessage messageType=Message2
    ``` channel
    loopback://four
    ```


|> SendMessage messageType=Message1, name=Tom
|> SendMessage messageType=Message2, name=Todd
|> SendMessage messageType=Message3, name=Trevor
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt                   |MessageType|Name  |
    |loopback://one  |Message1   |Tom   |
    |loopback://two  |Message2   |Todd  |
    |loopback://three|Message3   |Trevor|
    |loopback://four |Message2   |Todd  |
    |loopback://four |Message1   |Tom   |

~~~
