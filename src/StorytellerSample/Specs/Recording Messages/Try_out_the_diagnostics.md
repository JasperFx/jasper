# Try out the diagnostics

-> id = dc6279e2-fc7f-41fc-a5bf-ccfabd85dda7
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-09-15T13:11:35.3076670Z
-> tags = 

[Team]
|> CreateNewTeam team=Chiefs
|> CreateNewTeam team=Raiders
|> RecordGameResult day=TODAY-1, homeTeam=Chiefs, homeScore=21, visitorTeam=Raiders, visitorScore=14
|> RecordGameResult day=TODAY-2, homeTeam=Patriots, homeScore=-7, visitorTeam=Bengals, visitorScore=0
|> SendUnHandledMessage
~~~
