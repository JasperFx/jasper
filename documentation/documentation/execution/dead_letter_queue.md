<!--title:Dead Letter Envelopes-->

<[warning]>
You will need to use <[linkto:documentation/transports/durable]> to have the *dead letter queue* be persisted.
<[/warning]>

If a message cannot be processed after all its retries or if your <[linkto:documentation/execution/errorhandling;title=error handling policies]> explicitly use the `MoveToErrorQueue()` functionality, those envelopes are moved out of the active queues and saved
off to the side in your envelope storage.


## Retrieve an Error Report

It should be easy to browse the dead letter queue tables in your Postgresql database (`mt_doc_errorreport`) or your Sql Server database (`jasper_dead_letters`). If you know the envelope id of a dead letter envelope, you can use the `IEnvelopePersistor` interface in the IoC container of your application to
fetch the entire error report like this:

<[sample:FetchErrorReport]>