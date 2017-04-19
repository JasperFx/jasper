# Messages Sent to Multiple Channels

-> id = 052488cb-9641-44b3-9849-0e59109feec5
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-04-18T20:49:23.6930357Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1
    ``` channel
    memory://one
    ```

    |> SendMessage messageType=Message2
    ``` channel
    memory://two
    ```

    |> SendMessage messageType=Message3
    ``` channel
    memory://three
    ```

    |> SendMessage messageType=Message1
    ``` channel
    memory://four
    ```

    |> SendMessage messageType=Message2
    ``` channel
    memory://four
    ```


|> SendMessage messageType=Message1, name=Tom
|> SendMessage messageType=Message2, name=Todd
|> SendMessage messageType=Message3, name=Trevor
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt                   |MessageType|Name  |
    |memory://one  |Message1   |Tom   |
    |memory://two  |Message2   |Todd  |
    |memory://three|Message3   |Trevor|
    |memory://four |Message2   |Todd  |
    |memory://four |Message1   |Tom   |

~~~
