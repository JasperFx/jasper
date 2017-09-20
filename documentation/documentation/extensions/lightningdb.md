<!--title:Jasper.LightningDb-->

The Jasper.LightningDb extension adds a local persistence mechanism based on the [LMDB](http://www.lmdb.tech/doc/) library for
the <[linkto:documentation/messaging/transports/durable]>. Using this library gives you the ability to host a persistent, store and forward queueing mechanism within your application without any other additional infrastructure.

The only setup necessary to use this option is to have the proper `lmdb.dll` file in your binary directory (to a limited degree, the underlying [Lightning.Net](https://github.com/CoreyKaylor/Lightning.NET) library will try to extract and place an `lmdb.dll` based on your platform, but works best on Windows) and to install the Jasper.LightningDb assembly into your application through Nuget.

