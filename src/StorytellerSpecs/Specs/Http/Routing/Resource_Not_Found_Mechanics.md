# Resource Not Found Mechanics

-> id = f8a5de9e-e93f-4879-9b60-9b9d2fdb9b88
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-07-31T17:18:22.2939610Z
-> tags = 

[Router]
|> RoutesAre
    [table]
    |HttpMethod|Pattern      |
    |GET       |planets      |
    |GET       |planets/hoth |
    |GET       |planets/naboo|

|> TheResultShouldBe
    [table]
    |HttpMethod|Url               |Status|Body              |Arguments|
    |GET       |/planets/tattooine|404   |Resource Not Found|NONE     |
    |POST      |/planets          |404   |Resource Not Found|NONE     |

~~~
