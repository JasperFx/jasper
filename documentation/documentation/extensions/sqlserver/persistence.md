<!--title:Message Persistence-->

The message persistence requires and adds these tables to your schema:

1. `jasper_incoming_envelopes` - stores incoming and scheduled envelopes until they are successfully processed
1. `jasper_outgoing_envelopes` - stores outgoing envelopes until they are successfully sent through the transports
1. `jasper_dead_letters` - stores "dead letter" envelopes that could not be processed. See <[linkto:documentation/messaging/handling/dead_letter_queue]> for more information
1. `EnvelopeIdList` - table type that is used by some of the functions listed below

and also these functions that are all used by the durable messaging in its "message recovery" functionality:

1. `uspDeleteIncomingEnvelopes` 
1. `uspDeleteOutgoingEnvelopes` 
1. `uspDiscardAndReassignOutgoing` 
1. `uspMarkIncomingOwnership` 
1. `uspMarkOutgoingOwnership`

## Managing the Sql Server Schema

In testing, you can build -- or rebuild -- the message storage in your system with this syntax:

<[sample:SqlServer-RebuildMessageStorage]>

See [this GitHub issue](https://github.com/JasperFx/jasper/issues/372) for some utilities to better manage the database objects.