# Derive arguments from method signature

-> id = cf0cb7f8-8b05-4070-b0ce-83dee7839e53
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2019-01-10T18:50:29.8524600Z
-> tags = 

[RouteBuilder]
|> BuildRoute
    [table]
    |Method                             |HttpMethod|Pattern        |Arguments           |
    |get_person_name(name:string)       |GET       |person/:name   |name@1:string       |
    |get_query_from_to(from:int, to:int)|GET       |query/:from/:to|from@1:int, to@2:int|
    |post_user_id(id:Guid)              |POST      |user/:id       |id@1:Guid           |

~~~
