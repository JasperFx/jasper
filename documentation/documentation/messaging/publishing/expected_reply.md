<!--title:Send Message with Eventual Reply-->

In some cases you will send a message and expect a reply at some point from the downstream system, but you 
don't need the response right away and you don't care which node in your application handles the eventual reply. 
For that scenario, Jasper provides this functionality with the syntax shown below:

<[sample:using_global_request_and_reply]>

This functionality is different than the <[linkto:documentation/messaging/publishing/requestreply]> in that it doesn't wait for the 
response coming back from the receiving service in the call to `IMessageContext`. Use this mechanism to create simple workflows between
applications.

