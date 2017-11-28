# Reclaim Incoming Messages

-> id = 4f47205e-5c3e-46b5-874e-fbddd0a454eb
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-11-28T14:27:39.8262410Z
-> tags = 

[MessageRecovery]
|> EnvelopesAre
    [table]
    -> ExecutionTime = False
    -> DeliverBy = False
    -> Destination = False
    |Note|Id        |Status   |Owner     |
    |NULL|One       |Incoming |Any Node  |
    |NULL|Two       |Incoming |Any Node  |
    |NULL|Three     |Incoming |This Node |
    |NULL|Four      |Incoming |This Node |
    |NULL|Five      |Incoming |Third Node|
    |NULL|Six       |Incoming |Other Node|
    |NULL|Outgoing1 |Outgoing |Any Node  |
    |NULL|Scheduled1|Scheduled|Any Node  |


Should only recover incoming envelopes assigned to any node

|> AfterRecoveringIncomingMessages
|> ThePersistedEnvelopesOwnedByTheCurrentNodeAre
    [rows]
    |Id   |
    |One  |
    |Two  |
    |Three|
    |Four |

|> TheProcessedEnvelopesShouldBe
    [rows]
    |Id |
    |One|
    |Two|

~~~
