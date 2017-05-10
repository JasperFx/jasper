# Mixed Http Methods and Arguments

-> id = 7120895e-ffee-40fd-a580-4f3f4576936e
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-05-10T21:13:12.7568854Z
-> tags = 

[Router]
|> RoutesAre
    [table]
    |HttpMethod|Pattern        |
    |GET       |planets/:planet|
    |POST      |planets/:planet|
    |HEAD      |planets/naboo  |

|> TheResultShouldBe
    [table]
    |HttpMethod|Url           |Status|Body                  |Arguments    |
    |GET       |/planets/hoth |200   |GET: /planets/:planet |planet: hoth |
    |POST      |/planets/naboo|200   |POST: /planets/:planet|planet: naboo|
    |HEAD      |/planets/naboo|200   |HEAD: /planets/naboo  |NONE         |

~~~
