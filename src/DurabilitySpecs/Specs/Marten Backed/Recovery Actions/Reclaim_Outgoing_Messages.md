# Reclaim Outgoing Messages

-> id = 6061b23d-c646-4b9a-ac5c-0e35ce9b60df
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-11-28T14:28:44.9590450Z
-> tags = 

[MessageRecovery]
|> EnvelopesAre
    [table]
    |Note                              |Id     |Destination |ExecutionTime|DeliverBy|Status   |Owner     |
    |Happy Path                        |One    |stub://one  |NULL         |TODAY+1  |Outgoing |Any Node  |
    |Incoming message should be ignored|Two    |stub://one  |NULL         |TODAY+1  |Incoming |Any Node  |
    |Ignore scheduled message          |Three  |stub://one  |TODAY+1      |TODAY+1  |Scheduled|Any Node  |
    |Other Happy Path                  |Four   |stub://one  |NULL         |TODAY+1  |Outgoing |Any Node  |
    |Other Happy Path                  |Five   |stub://two  |NULL         |TODAY+1  |Outgoing |Any Node  |
    |Other Happy Path                  |Six    |stub://two  |NULL         |TODAY+1  |Outgoing |Any Node  |
    |Node is latched                   |Latched|stub://three|NULL         |TODAY+1  |Outgoing |Any Node  |
    |Owned by another node             |Seven  |stub://two  |NULL         |TODAY+1  |Outgoing |Other Node|
    |Already owned by the current node |Eight  |stub://one  |NULL         |TODAY+1  |Outgoing |This Node |
    |Expired message                   |Nine   |stub://one  |NULL         |TODAY-1  |Outgoing |Any Node  |

|> ChannelIsLatched channel=stub://three
|> AfterExecutingTheOutgoingMessageRecovery

Should take over ownership of envelopes that were previously "any node" that were eligible to be sent. Already owned "Eight"

|> ThePersistedEnvelopesOwnedByTheCurrentNodeAre
    [rows]
    |Id   |
    |One  |
    |Four |
    |Five |
    |Six  |
    |Eight|


"Nine" is expired, so we'll just delete it as we go. Do nothing w/ channels that are latched

|> ThePersistedEnvelopesOwnedByAnyNodeAre
    [rows]
    |Id     |
    |Two    |
    |Three  |
    |Latched|

|> TheEnvelopesSentShouldBe
    [rows]
    |Id  |Destination|
    |One |stub://one |
    |Four|stub://one |
    |Five|stub://two |
    |Six |stub://two |

~~~
