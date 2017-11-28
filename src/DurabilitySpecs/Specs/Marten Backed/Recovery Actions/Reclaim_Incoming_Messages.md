# Reclaim Incoming Messages

-> id = 4f47205e-5c3e-46b5-874e-fbddd0a454eb
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-11-28T13:48:26.5265770Z
-> tags = 

[MessageRecovery]
|> EnvelopesAre
    [table]
    -> ExecutionTime = False
    -> DeliverBy = False
    -> Destination = False
    |Id        |Status   |Owner     |
    |One       |Incoming |Any Node  |
    |Two       |Incoming |Any Node  |
    |Three     |Incoming |This Node |
    |Four      |Incoming |This Node |
    |Five      |Incoming |Third Node|
    |Six       |Incoming |Other Node|
    |Outgoing1 |Outgoing |Any Node  |
    |Scheduled1|Scheduled|Any Node  |


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
