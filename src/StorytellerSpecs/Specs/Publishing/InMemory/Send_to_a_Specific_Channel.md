# Send to a Specific Channel

-> id = 44c40530-b853-4521-ad3f-45ca12365a5b
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-04-18T20:34:18.0228532Z
-> tags = 

[SendMessage]
|> IfTheApplicationIs
    [ServiceBusApplication]
    |> SendMessage messageType=Message1, channel=stub://one
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


|> SendMessageDirectly messageType=Message1, name=Hank
``` address
memory://three
```

|> TheMessagesSentShouldBe
    [rows]
    |> TheMessagesSentShouldBe-row MessageType=Message1, Name=Hank
    ``` ReceivedAt
    memory://three
    ```


~~~
