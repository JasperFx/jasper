# Reclaim Incoming Messages

-> id = 4f47205e-5c3e-46b5-874e-fbddd0a454eb
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-12-11T20:45:07.2741600Z
-> tags = 

[MessageRecovery]
|> EnvelopesAre
    [table]
    -> ExecutionTime = False
    -> DeliverBy = False
    -> Destination = False
    |Note|Id                                  |Status   |Owner     |
    |NULL|44b6e63b-232d-4124-9737-8c4d0350fe71|Incoming |Any Node  |
    |NULL|44b6e63b-232d-4124-9737-8c4d0350fe72|Incoming |Any Node  |
    |NULL|44b6e63b-232d-4124-9737-8c4d0350fe73|Incoming |This Node |
    |NULL|44b6e63b-232d-4124-9737-8c4d0350fe74|Incoming |This Node |
    |NULL|44b6e63b-232d-4124-9737-8c4d0350fe75|Incoming |Third Node|
    |NULL|44b6e63b-232d-4124-9737-8c4d0350fe76|Incoming |Other Node|
    |NULL|44b6e63b-232d-4124-9737-8c4d0350fe00|Outgoing |Any Node  |
    |NULL|44b6e63b-232d-4124-9737-8c4d0350fe01|Scheduled|Any Node  |


Should only recover incoming envelopes assigned to any node

|> AfterRecoveringIncomingMessages
|> ThePersistedEnvelopesOwnedByTheCurrentNodeAre
    [rows]
    |Id                                  |
    |44b6e63b-232d-4124-9737-8c4d0350fe71|
    |44b6e63b-232d-4124-9737-8c4d0350fe72|
    |44b6e63b-232d-4124-9737-8c4d0350fe73|
    |44b6e63b-232d-4124-9737-8c4d0350fe74|

|> TheProcessedEnvelopesShouldBe
    [rows]
    |Id                                  |
    |44b6e63b-232d-4124-9737-8c4d0350fe71|
    |44b6e63b-232d-4124-9737-8c4d0350fe72|

~~~
