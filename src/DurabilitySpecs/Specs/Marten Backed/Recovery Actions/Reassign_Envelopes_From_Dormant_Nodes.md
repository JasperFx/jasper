# Reassign Envelopes From Dormant Nodes

-> id = 382d32db-88c6-4db9-a69e-38fd509f0d9b
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-11-28T14:59:45.7092260Z
-> tags = 

[MessageRecovery]
|> EnvelopesAre
    [table]
    -> ExecutionTime = False
    -> DeliverBy = False
    -> Destination = False
    |Note|Id   |Status  |Owner     |
    |NULL|One  |Incoming|This Node |
    |NULL|Two  |Incoming|This Node |
    |NULL|Three|Outgoing|Other Node|
    |NULL|Four |Outgoing|Other Node|
    |NULL|Five |Outgoing|Any Node  |
    |NULL|Six  |Outgoing|Any Node  |
    |NULL|Seven|Outgoing|Third Node|
    |NULL|Eight|Incoming|Third Node|

|> NodeIsActive node=Third Node

Third Node is active, so the only envelopes that should be "recovered" are those that are marked as being owned by 'Other Node'

|> AfterReassigningFromDormantNodes
|> ThePersistedEnvelopesOwnedByAnyNodeAre
    [rows]
    |Id   |
    |Three|
    |Four |
    |Five |
    |Six  |

~~~
