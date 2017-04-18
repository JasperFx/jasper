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
    memory://localhost:2201/one
    ```

    |> SendMessage messageType=Message2
    ``` channel
    memory://localhost:2201/two
    ```

    |> SendMessage messageType=Message3
    ``` channel
    memory://localhost:2201/three
    ```

    |> SendMessage messageType=Message1
    ``` channel
    memory://localhost:2201/four
    ```

    |> SendMessage messageType=Message2
    ``` channel
    memory://localhost:2201/four
    ```


|> SendMessage messageType=Message1, name=Tom
|> SendMessage messageType=Message2, name=Todd
|> SendMessage messageType=Message3, name=Trevor
|> TheMessagesSentShouldBe
    [rows]
    |ReceivedAt                   |MessageType|Name  |
    |memory://localhost:2201/one  |Message1   |Tom   |
    |memory://localhost:2201/two  |Message2   |Todd  |
    |memory://localhost:2201/three|Message3   |Trevor|
    |memory://localhost:2201/four |Message2   |Todd  |
    |memory://localhost:2201/four |Message1   |Tom   |

~~~
