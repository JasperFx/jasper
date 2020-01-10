<!--title:Execution Pipeline-->

Jasper's execution pipeline for handling messages is unique. Instead of forcing you to constrain your code to meet Jasper's 
interfaces, Jasper tries to adapt itself to **your** code. It does this by taking your <[linkto:documentation/execution/handlers]> and 
whatever <[linkto:documentation/execution/middleware_and_codegen]>, then generating C# code to call your code in the most efficient way
possible using Roslyn's ability to compile code at runtime.

The following topics will help you understand how to build and customize Jasper message handlers:

<[TableOfContents]>