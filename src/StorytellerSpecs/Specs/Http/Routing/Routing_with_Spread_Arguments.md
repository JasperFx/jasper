# Routing with Spread Arguments

-> id = 6fa8e393-de9c-43be-9771-43abfbe3efeb
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-11-07T13:22:52.5874950Z
-> tags = 

[Routing]
|> RoutesAre
    [table]
    |Route             |
    |EMPTY             |
    |...               |
    |folder/...        |
    |planets           |
    |file/...          |
    |customer/:name/...|

|> TheSelectionShouldBe
    [table]
    |Url                 |Selected          |Arguments                   |
    |EMPTY               |EMPTY             |NONE                        |
    |planets             |planets           |NONE                        |
    |file                |file/...          |spread: empty               |
    |file/a              |file/...          |spread: a                   |
    |file/a/b            |file/...          |spread: a, b                |
    |file/a/b/c          |file/...          |spread: a, b, c             |
    |folder/a/b/c        |folder/...        |spread: a, b, c             |
    |customer/BigCo      |customer/:name/...|name: BigCo; spread: empty  |
    |customer/BigCo/1/2/3|customer/:name/...|name: BigCo; spread: 1, 2, 3|

~~~
