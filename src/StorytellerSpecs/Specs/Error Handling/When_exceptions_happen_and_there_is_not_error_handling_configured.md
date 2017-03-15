# When exceptions happen and there is not error handling configured

-> id = 7a695ad2-f511-411b-9f98-e7180b441e8a
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-03-15T16:24:03.3491771Z
-> tags = 

[ErrorHandling]
|> MessageAttempts
    [Rows]
    |> MessageAttempts-row Attempt=1, errorType=DivideByZeroException

|> MessageResult attempt=1, result=MovedToErrorQueue
~~~
