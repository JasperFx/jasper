# Reclaim Outgoing Messages

-> id = 6061b23d-c646-4b9a-ac5c-0e35ce9b60df
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2017-12-11T20:46:42.3207650Z
-> tags = 

[MessageRecovery]
|> EnvelopesAre
    [table]
    |Note                              |Id                                  |Destination |ExecutionTime|DeliverBy|Status   |Owner     |
    |Happy Path                        |44b6e63b-232d-4124-9737-8c4d0350fe71|stub://one  |NULL         |TODAY+1  |Outgoing |Any Node  |
    |Incoming message should be ignored|44b6e63b-232d-4124-9737-8c4d0350fe72|stub://one  |NULL         |TODAY+1  |Incoming |Any Node  |
    |Ignore scheduled message          |44b6e63b-232d-4124-9737-8c4d0350fe73|stub://one  |TODAY+1      |TODAY+1  |Scheduled|Any Node  |
    |Other Happy Path                  |44b6e63b-232d-4124-9737-8c4d0350fe74|stub://one  |NULL         |TODAY+1  |Outgoing |Any Node  |
    |Other Happy Path                  |44b6e63b-232d-4124-9737-8c4d0350fe75|stub://two  |NULL         |TODAY+1  |Outgoing |Any Node  |
    |Other Happy Path                  |44b6e63b-232d-4124-9737-8c4d0350fe76|stub://two  |NULL         |TODAY+1  |Outgoing |Any Node  |
    |Node is latched                   |44b6e63b-232d-4124-9737-8c4d0350fe00|stub://three|NULL         |TODAY+1  |Outgoing |Any Node  |
    |Owned by another node             |44b6e63b-232d-4124-9737-8c4d0350fe77|stub://two  |NULL         |TODAY+1  |Outgoing |Other Node|
    |Already owned by the current node |44b6e63b-232d-4124-9737-8c4d0350fe78|stub://one  |NULL         |TODAY+1  |Outgoing |This Node |
    |Expired message                   |44b6e63b-232d-4124-9737-8c4d0350fe79|stub://one  |NULL         |TODAY-1  |Outgoing |Any Node  |

|> ChannelIsLatched channel=stub://three
|> AfterExecutingTheOutgoingMessageRecovery

Should take over ownership of envelopes that were previously "any node" that were eligible to be sent. Already owned "Eight"

|> ThePersistedEnvelopesOwnedByTheCurrentNodeAre
    [rows]
    |Id                                  |
    |44b6e63b-232d-4124-9737-8c4d0350fe71|
    |44b6e63b-232d-4124-9737-8c4d0350fe74|
    |44b6e63b-232d-4124-9737-8c4d0350fe75|
    |44b6e63b-232d-4124-9737-8c4d0350fe76|
    |44b6e63b-232d-4124-9737-8c4d0350fe78|


"Nine" is expired, so we'll just delete it as we go. Do nothing w/ channels that are latched

|> ThePersistedEnvelopesOwnedByAnyNodeAre
    [rows]
    |Id                                  |
    |44b6e63b-232d-4124-9737-8c4d0350fe72|
    |44b6e63b-232d-4124-9737-8c4d0350fe73|
    |44b6e63b-232d-4124-9737-8c4d0350fe00|

|> TheEnvelopesSentShouldBe
    [rows]
    |Id                                  |Destination|
    |44b6e63b-232d-4124-9737-8c4d0350fe71|stub://one |
    |44b6e63b-232d-4124-9737-8c4d0350fe74|stub://one |
    |44b6e63b-232d-4124-9737-8c4d0350fe75|stub://two |
    |44b6e63b-232d-4124-9737-8c4d0350fe76|stub://two |

~~~
