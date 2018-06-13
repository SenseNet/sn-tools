# SenseNet Diagnostics
In this namespace you'll find easy-to-use and extendable logging components. We build on them extensively in the core SenseNet project but they can also be used in any kind of tool or application as a lightweight logging technology.

## SnLog
This component was designed to handle the most common event logging scenarios - e.g. writing messages or exceptions to the underlying replaceable adapter.

If you want to use this logger in a custom tool or application, please provide the appropriate logger instance during app start:
```csharp
// built-in logger based on Enterprise Library   
SnLog.Instance = new EntLibLoggerAdapter();

// built-in lightweight logger for simple tools
SnLog.Instance = new TraceEventLogger();
```
(of course SenseNet already does this for you, you do not have to do this when you are working in the context of SenseNet)
#### Usage
Writing a message to the logger is a one line operation. There are a couple of optional parameters (e.g. category or properties) that you can fill to provide more information to the collegue who will review the entries later.
```csharp
SnLog.WriteInformation("ContentTypeManager loaded.");
SnLog.WriteException(ex, "Error during content save.", EventId.Indexing, 
   properties: new Dictionary<string, object> { { "Path", path } });
```
Providing an event id also helps debugging errors; you can even use one of the built-in ids listed in the *EventId* static class - or define your own ids.


## SnTrace
Tracing is a bit different subject. It requires a lightning fast infrastructure and can be used a lot more extensively in your code than writing events. In SenseNet we write trace messages in many places - e.g. at web request life cycle events, db operations or content life cycle events.

```csharp
// this will go to the Custom category
SnTrace.Write("My message");

// this will be written to the Database category
SnTrace.Database.Write("Query executed");
```

We have a predefined list of categories and you can either use the Custom category or define your own ones on-the-fly. Categories can be switched on or off to make the trace log less noisy.

The trace lines will be written to the file system in a buffered way so that it does not take too much resources.

For details please visit [this article](/docs/sntrace.md).