<!--title:Request / Reply -->

From [Wikipedia](https://en.wikipedia.org/wiki/Request%E2%80%93response):

> Request–response, or request–reply, is one of the basic methods computers use to communicate with each other, in which the first computer sends a request for some data and the second computer responds to the request. Usually, there is a series of such interchanges until the complete message is sent; browsing a web page is an example of request–response communication. Request–response can be seen as a telephone call, in which someone is called and they answer the call.

While you'll probably use HTTP services more often than not for request/reply, you also have the option to use the messaging support like this:

<[sample:using-request-reply]>

In this case, we're sending a `Ping` message and getting a `Task<Pong>` object back to track
the expected response. This operation works by sending a message with header values denoting that the original sender is interested in a response. When the response is successfully received back
in the original sender, Jasper will set the result of the `Task<Pong>`. 

Definitely note that this operation will throw a timeout exception if the reply is not received within the set threshold (10 seconds by default).

To customize the timeout or even the destination for the request, you have this syntax:

<[sample:CustomizedRequestReply]>

To add some more context, the request in the receiving system should have a handler that
returns a `Pong` message as a cascading message like this one:

<[sample:PingPongHandler]>
