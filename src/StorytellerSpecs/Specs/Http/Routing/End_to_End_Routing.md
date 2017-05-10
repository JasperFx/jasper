# Router can differentiate between methods

-> id = 2d2a08cd-b0ff-42ef-a004-ae94b337f2dc
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-05-10T21:13:12.7578854Z
-> tags = 

[Router]
|> RoutesAre
    [table]
    |HttpMethod|Pattern|
    |GET       |EMPTY  |
    |POST      |EMPTY  |
    |DELETE    |EMPTY  |
    |PUT       |EMPTY  |
    |HEAD      |EMPTY  |

|> TheResultShouldBe
    [table]
    |HttpMethod|Url  |Status|Body     |Arguments|
    |GET       |EMPTY|200   |GET: /   |NONE     |
    |POST      |EMPTY|200   |POST: /  |NONE     |
    |DELETE    |EMPTY|200   |DELETE: /|NONE     |
    |PUT       |EMPTY|200   |PUT: /   |NONE     |
    |HEAD      |EMPTY|200   |HEAD: /  |NONE     |

~~~
