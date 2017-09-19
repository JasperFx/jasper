<!--title:Publishing Messages-->

The subjects in this section are about how to send messages and even customize how the messages should be delivered. See <[linkto:documentation/messaging/routing]> for information about how
to subscribe to message or set up routing rules to control *where* your messages are sent.

Jasper supports a couple different patterns of messaging:

* <[linkto:documentation/messaging/publishing/pubsub]> - sends a message to one or more subscribers
* <[linkto:documentation/messaging/publishing/requestreply]> - send a message to another system while expecting a response from that system
* <[linkto:documentation/messaging/publishing/invoke]> - handle a message locally either inline or by enqueueing the message locally
* <[linkto:documentation/messaging/publishing/delayed]> - publish a message with the expectation that it be processed at a later time
* <[linkto:documentation/messaging/publishing/customizing_envelopes]> - take full control about how or where a message is sent 