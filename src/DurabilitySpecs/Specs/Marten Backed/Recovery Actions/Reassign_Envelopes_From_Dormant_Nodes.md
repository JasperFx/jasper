# Reassign Envelopes From Dormant Nodes

-> id = 382d32db-88c6-4db9-a69e-38fd509f0d9b
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-12-11T20:44:04.6073490Z
-> tags = 

[MessageRecovery]
|> EnvelopesAre
    [table]
    -> ExecutionTime = False
    -> DeliverBy = False
    -> Destination = False
    |Note|Id                                  |Status  |Owner     |
    |NULL|44b6e63b-232d-4124-9737-8c4d0350fe71|Incoming|This Node |
    |NULL|44b6e63b-232d-4124-9737-8c4d0350fe72|Incoming|This Node |
    |NULL|44b6e63b-232d-4124-9737-8c4d0350fe73|Outgoing|Other Node|
    |NULL|44b6e63b-232d-4124-9737-8c4d0350fe74|Outgoing|Other Node|
    |NULL|44b6e63b-232d-4124-9737-8c4d0350fe75|Outgoing|Any Node  |
    |NULL|44b6e63b-232d-4124-9737-8c4d0350fe76|Outgoing|Any Node  |
    |NULL|44b6e63b-232d-4124-9737-8c4d0350fe77|Outgoing|Third Node|
    |NULL|44b6e63b-232d-4124-9737-8c4d0350fe78|Incoming|Third Node|

|> NodeIsActive node=Third Node

Third Node is active, so the only envelopes that should be "recovered" are those that are marked as being owned by 'Other Node'

|> AfterReassigningFromDormantNodes
|> ThePersistedEnvelopesOwnedByAnyNodeAre
    [rows]
    |Id                                  |
    |44b6e63b-232d-4124-9737-8c4d0350fe73|
    |44b6e63b-232d-4124-9737-8c4d0350fe74|
    |44b6e63b-232d-4124-9737-8c4d0350fe75|
    |44b6e63b-232d-4124-9737-8c4d0350fe76|

~~~
