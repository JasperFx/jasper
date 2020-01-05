<!--title:Message Persistence-->

The message persistence requires and adds these tables to your schema:

1. `jasper_incoming_envelopes` - stores incoming and scheduled envelopes until they are successfully processed
1. `jasper_outgoing_envelopes` - stores outgoing envelopes until they are successfully sent through the transports
1. `jasper_dead_letters` - stores "dead letter" envelopes that could not be processed. See <[linkto:documentation/messaging/handling/dead_letter_queue]> for more information

## Managing the Postgresql Schema

In testing, you can build -- or rebuild -- the message storage in your system with a call to the `RebuildMessageStorage() ` extension method off of either `IWebHost` or `IJasperHost` as shown below in a sample taken from xUnit integration testing with Jasper:

<[sample:MyJasperAppFixture]>

See [this GitHub issue](https://github.com/JasperFx/jasper/issues/372) for some utilities to better manage the database objects.