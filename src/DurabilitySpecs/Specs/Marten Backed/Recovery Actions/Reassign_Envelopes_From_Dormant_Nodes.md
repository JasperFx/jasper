# Reassign Envelopes From Dormant Nodes

-> id = 382d32db-88c6-4db9-a69e-38fd509f0d9b
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-11-28T13:32:00.0801350Z
-> tags = 

[MessageRecovery]
|> EnvelopesAre
    [table]
    -> ExecutionTime = False
    -> DeliverBy = False
    -> Destination = False
    |Id   |Status  |Owner     |
    |One  |Incoming|This Node |
    |Two  |Incoming|This Node |
    |Three|Outgoing|Other Node|
    |Four |Outgoing|Other Node|
    |Five |Outgoing|Any Node  |
    |Six  |Outgoing|Any Node  |
    |Seven|Outgoing|Third Node|
    |Eight|Incoming|Third Node|

|> NodeIsActive node=Third Node

Third Node is active, so the only envelopes that should be "recovered" are those that are marked as being owned by 'Other Node'

|> ThePersistedEnvelopesOwnedByAnyNodeAre
    [rows]
    |Id   |
    |Three|
    |Four |
    |Five |
    |Six  |

~~~
