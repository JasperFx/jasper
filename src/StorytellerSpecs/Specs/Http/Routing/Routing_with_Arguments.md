# Routing with Arguments

-> id = eab1ea55-df13-488f-8e16-1490e3abb7f4
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-11-07T13:22:52.5839760Z
-> tags = 

[Routing]
|> RoutesAre
    [table]
    |Route             |
    |EMPTY             |
    |colors            |
    |colors/:color     |
    |colors/all        |
    |query/:from/to/:to|
    |:planet           |

|> TheSelectionShouldBe
    [table]
    |Url          |Selected          |Arguments      |
    |EMPTY        |EMPTY             |NONE           |
    |colors       |colors            |NONE           |
    |colors/all   |colors/all        |NONE           |
    |colors/red   |colors/:color     |color: red     |
    |colors/green |colors/:color     |color: green   |
    |query/1/to/5 |query/:from/to/:to|from: 1; to: 5 |
    |query/5/to/10|query/:from/to/:to|from: 5; to: 10|

~~~
