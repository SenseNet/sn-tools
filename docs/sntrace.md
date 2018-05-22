---
title: "Tracing"
source_url: 'https://github.com/SenseNet/sn-tools/blob/master/docs/sntrace.md'
category: Development
version: v1.0
tags: [trace, verbose log, detailed log, debug]
description: Tracing is an important toolset of a complex system that enables exploring bugs, performance problems or other operational anomalies.
---

# SnTrace
Every advanced software needs a tool that provides information about the state of the application and what is happening inside. Another important maintenance capability is to measure performance, e.g. the duration of key operations. In sensenet the SnTrace component is a base of an easy-to-use tracing tool and measuring instrument in one.

For details about logging in general please visit the main [Logging article](http://wiki.sensenet.com/Logging).

## Details
First of all, SnTrace is a quick *verbose logger*. This feature is different from event logging such as writing errors and warnings or audit events. Verbose logging needs to be as fast as possible regardless of whether the feature is turned on or off. It is designed to be able to handle even a huge load of concurrent events and is usually used to monitor the behavior of the system when there are lots of things going on. For example tracing is reponsible for marking all important points of a content life cycle (e.g. saving the data to the Content Repository, than to the security db, than indexing, etc.), while an audit log entry is a single record in the audit log about a content that was created. The main features of the tracing in sensenet:
- Writing trace with the SnTrace API
  - Writing **single line** entries to read and process easy way.
  - Using **operations** to measure logical business steps.
  - Using **categories** to reduct the noise.
- Developing a custom trace provider or using our built-in tracers
  - Implementing **ISnTracer** or **BufferedSnTracer**.
  - Built-in tracers: **SnFileSystemTracer**, **SnDebugViewTracer**.
- Analyzing traces
  - Parsing, filtering, associating, transforming entries (coming soon).
- Monitoring trace
  - Using **SnDebugViewTracer** and **[DebugView](https://docs.microsoft.com/en-us/sysinternals/downloads/debugview#introduction)** of *[Sysinternals](https://docs.microsoft.com/en-us/sysinternals/)* ([download](https://docs.microsoft.com/en-us/sysinternals/downloads/debugview)).

### The data we collect
Calling the *SnTrace.Write* method records a simple text line containing the target message extended by TAB separated generalized sytem information. This line is called a *Trace Entry* (see below).

We try to place SnTrace.Write calls on every important point in the system: subsystem events (e.g. messaging or security), content life cycle (milestones of content creation like indexing), executed queries, db operations, etc. We organized these events into different categories that can be switched on or off individually. For details see the *Categories* section below.

Another important task of SnTrace is **performance measurement**. It is based on a very simple API that is able to write the start and finish time of an operation (e.g. Save) as two separate lines to the trace. Both lines contain a correlation id and the finish line contains the **duration** too.

This technique is quick enough, the log lines are human readable and the generator code is very simple (see the examples below).

> According to our measurements, the system deceleration rate is 1% on a loaded web server and high verbosity logging. Still, it is advisable to switch tracing on when needed and switch it off after.

### Where does the data go
The trace entries are written to various targets depending on the trace providers that are currently active. A provider needs to implement the very simple **ISnTracer** interface. Multiple providers can be used simultaneously. See details of implementation below. The default SnTrace provider is the **SnFileSystemTracer** that persists the entries to the file system (using a buffer) as fast as possible.

## Trace entry
A trace stream contains single line entries. An entry has TAB separated fields in fixed order. This format ensures that these lines can be quickly processed if they are placed into a new Excel worksheet.

```text
Line  Time                       Category  ApplicationName          Thread  Op      State  Duration         Message
----- -------------------------  --------  -----------------------  ------  ------  -----  ---------------  -------------------------------
>2461 2016-04-25 07:07:10.71858  Database  A:/LM/W3SVC/2/ROOT-2...  T:357                                   Transaction BEGIN: 91.
2462  2016-04-25 07:07:10.71858  Database  A:/LM/W3SVC/2/ROOT-2...  T:357   Op:725  Start                   SqlProcedure.ExecuteNonQuery...
2463  2016-04-25 07:07:10.73960  Database  A:/LM/W3SVC/2/ROOT-2...  T:357   Op:725  End    00:00:00.021022  SqlProcedure.ExecuteNonQuery...
```

A trace line has a strict format which is a TAB separated list of fields:
1. **LineId**: Identifier number of the line. Unique in the AppDomain. If the line starts with the '>' character, this is the first line in the block that was written to the disk in one step.
2. **Time**: Creation time of the line in ISO 8601 format with this specifier: `yyyy-MM-dd HH:mm:ss.ffffff`.
3. **Category**: Trace category (see below).
4. **ApplicationName**: AppDomain name: **"A:UnitTestAdapter"**. Helps identifying the Application if more trace streams are merged and sorted by time.
5. **ThreadId**: Current thread id: **"T:9856"**.
6. **Operation**: Id of the operation (see below): **"Op:421"**
7. **Status**: Value can be empty, "Start", "End", "UNTERMINATED" or "ERROR".
8. **Duration**: Duration if this line is the end of an operation (only operation end writes this value).
9. **Message**: The subject of the line. Raw message provided by the developer.

### Writing messsages
Writing messages as simply as possible. Use format strings and parameters.
Use correlation data (e.g. #id, @user etc.).
```csharp
var content = Content.CreateNew("TestContent", TestRoot, "test-content-1");
content.Save();
SnTrace.Write("Test content created. Id: {0}", content.Id);
```
Newlines and any other unprintable characters except tabs are replaced with the "." character. Tab character is not forbidden in the message but it should be used carefully. If the log lines are pasted into a spreadsheet software (e.g. MS Excel) the tabs can cause unwanted columns in the table.

The Write method uses smart formatting: enumerable arguments appear in a json array like format:
```csharp
SnTrace.Write("Values: {0}", new List<int>(new[] { 1, 2, 3, 4 }));
SnTrace.Write("Values: {0}", new List<string>(new[] { "one", "two", "three" }));
```
```
Line Time            Message
---- ---------- ...  -------------------------
>1   2018-05-18 ...  Values: [1, 2, 3, 4]
2    2018-05-18 ...  Values: [one, two, three]
```

### Writing errors
Write error messages if the line contains details about a mistake. In this case the status field of the trace entry will be "ERROR".
```csharp
try
{
    WrongOperation();
}
catch(Exception e)
{
    SnTrace.WriteError("Error message");
}
```
```text
Line   Time    Category  App  Thr  Op  State  Duration  Message
-----  ------  --------  ---  ---  --  -----  --------  -------------
2462   2016..  Custom    A:.  T:3      ERROR            Error message
```
### Measuring operations
An incredibly useful feature of tracing is that it contains a simple API for measuring how much time it takes to execute a block of code (not necessarily a whole method).

In the code, the operation is usually represented by a `using` block:

```csharp
using (var op = SnTrace.StartOperation("Operation-1"))
{
    ...
    op.Successful = true;
}
```

In the trace the operation will manifest as two lines: operation start and end. The state column differentiates the lines by the "Start" or "End" words. The duration field in the operation end line contains the elapsed time since the start line. Both lines contain the *same operation id* (OpId) and message. The id is an integer number that is unique in the appdomain's lifecycle. Because the end line duplicates the start line's message, operation messages should be a short description of the program execution step, for example: "Save Content #1234", "Load MyFeature's configuration", etc.
```text
Line Time  Category App Thr  Op    State         Duration         Message
---- ----- -------- --- ---- ----  ------------  ---------------  ---------------------
>1   2016- Custom   A:. T:6  Op:1  Start                          Operation-1
2    2016- Custom   A:. T:6  Op:1  End	         00:00:00.000007  Operation-1
```

Operations can be nested. The levels of operations will not appear in the trace so please make sure you use easy to understand operation messages.

An operation's start line contains "Start" in the state field. If the operation is successfully finished, the end line contains the "End" state and the duration of the operation. Otherwise the state is "UNTERMINATED".

```csharp
using (var op = SnTrace.StartOperation("Measured outer block"))
{
    Thread.Sleep(42);
    using (var op1 = SnTrace.StartOperation("Measured inner block"))
    {
        Thread.Sleep(42);
        //op1.Successful = true;
    }
    Thread.Sleep(42);
    op.Successful = true;
}
```
As you can see, developers have to explicitly state that the operation was successful by **setting the Successful flag on the operation object**. If this does not happen (as in case of the inner block in the code above - see the commented line), the operation is considered unsuccessful and this gets written into the log. This technique helps the system work even in case of 'hidden' program flows (e.g. exceptions or returns from the middle of a method). The result of the code above will be this:
```text
Line Time  Category App Thr  Op    State         Duration         Message
---- ----- -------- --- ---- ----  ------------  ---------------  ---------------------
>1   2016- Custom   A:. T:6  Op:1  Start                          Measured outer block
2    2016- Custom   A:. T:6  Op:2  Start                          Measured inner block
3    2016- Custom   A:. T:6  Op:2  UNTERMINATED  00:00:00.044005  Measured inner block
4    2016- Custom   A:. T:6  Op:1  End	         00:00:00.130007  Measured outer block
```
## Categories
Every trace line is categorized. There are three kinds of categories:
- Default
- Built-in
- User defined

Every category can be switched on or off from code.
```csharp
SnTrace.Web.Enabled = true;
SnTrace.EnableAll();
SnTrace.DisableAll();
```
The built-in categories can be managed also via the trace setting content (`/Root/System/Settings/Logging.settings`). Default content of this setting:
```javascript
Trace: {
    ContentOperation: false,
    Database: false,
    Index: false,
    IndexQueue: false,
    Query: false,
    Repository: false,
    Messaging: false,
    Security: false,
    SecurityQueue: false,
    Web: false,
    Workflow: false,
    System: false,
    TaskManagement: false,
    Test: false,
    Event: false,
    Custom: false,
}
```

Modifying this content needs administrator privileges. To modify this content via an OData request (instead of the user interface) log in as an administrator and send the following request:

```javascript
$.ajax({
    url: "odata.svc/Root/System/Settings('Logging.settings')",
    dataType: "json",
    type: 'PATCH',
    data: "models=[" + JSON.stringify({ 'Trace.Web':true}) + "]",
    success: function () {
        console.log('Content is successfully patched');
    }
});
```

#### Default category
If you do not specify a category, it will be written to the "Custom" built-in category.

```csharp
SnTrace.Write("Not categorized message");
```
In this case the category column shows the "Custom" word.
```text
12345   2018-05-17 23:20:16.57025   Custom        ....  Not categorized message
```
#### Built-in categories
There are a nuber of built-in categories used by the system. For example using the "Test" category: 
```csharp
SnTrace.Test.Write("Message for a test purpose.");
```
...will write the following line:
```text
12346   2018-05-17 23:20:16.57025   Test          ....  Message for a test purpose.
```
The list of built-in categories may grow in the future. In this version we use these:
- **ContentOperation**: Records content related operations e.g. content saving, templated creation, checkout etc.
- **Database**: Many sql procedures and transactions are measured. Deadlocks are also logged.
- **Event**: Includes events sent through the regular Logging API (see the Event logging section below).
- **Index**: Traces the indexing process.
- **IndexQueue**: Traces the indexing activity flow.
- **Messaging**: Helps monitoring the messaging subsystem (e.g. receiving and sending messages).
- **Query**: Measures content queries and writes some kernel information.
- **Repository**: Writes repository system level messages, start and shut down information.
- **Security**: Security related operations and events.
- **SecurityQueue**: Traces the security activity flow.
- **System**: Traces the start-stop sequences and system composition messages.
- **TaskManagement**: Records operations in connection with the TaskManagement subsystem including the TaskManagementWeb, Service and Agent too.
- **Test**: Unit testing uses this category. Measures the execution time of every test method and the whole test session.
- **Web**: Writes the requested url and HTTP method.
- **Workflow**: Records background operations of the workflow subsystem.

#### User defined categories
It is possible to write to user defined categories via calling the Category(name) method. This method returns with a fully functional, dynamic category created on-the-fly. For example:
```csharp
SnTrace.Category("MyCategory").Write("MyMessage");
```
The result in the trace log:
```text
Line Time          Category   AppN Thr Op St Du Message
---- ------------- ---------- ---- --- -- -- -- ----------------
1... 2016-04-27... MyCategory A:.. T:6          Custom text
```
In a better solution the user defined category should be pinned in a globally accessible variable:
```csharp
SnTrace.SnTraceCategory MyCategory = SnTrace.Category("MyCategory");
```
And using this category without a magic string:
```csharp
MyCategory.Write("MyMessage");
```
The pinned dynamic category can be switched on- or off on the created instance but the initial state is bound to the Enabled property of the Custom category. Except for the main switch, the dynamic category knows everything that the built-in copies so it can write messages and errors and can handle operations too. In the following example the 2nd line is not written to the output because the category is switched off temporarily.
```csharp
MyCategory.Enabled = false; // or true
```
#### Event logging
When you want to monitor an application, errors and warnings are among the most important information so these log entries should not be missing from the detailed log. You can include these entries in the verbose log by switching on the Event category (see above).

Because these events may contain a huge amount of text, SnTrace saves only the message of the log entry. If you want to connect the trace line to the full event message (that can be found in the Event log), use the **GUID** added to every event line in the trace. The same GUID is also included in the related event data (extended properties section, SnTrace property).
```text
SnTrace - '''#01d500aa-b8c8-423e-8a4a-ecedafd0c936'''
```
In the detailed log the association id is in the Message after the event classifier:
```text
Line Time   Cat.. ... Message
---- ------ ----- ... ----------------------------------------------------------------
5614 2018.. Event ... ERROR '''#01d500aa-b8c8-423e-8a4a-ecedafd0c936''': Invalid query
```

## Customizing trace providers
The SnTrace API is responsible for managing categories, formatting the final trace message and calling one or more tracer. Persisting, visualizing these messages or sending them to other places are the responsibilities of trace providers. These providers need to be fast, fault-tolerant components.

The active providers can be configured by a static list. In this version we use this default setting:
```csharp
SnTracers = new List<ISnTracer>(new[] { new SnFileSystemTracer() });
```
The list cannot be changed but items are freely changeable and replaceable.

A trace writer needs to implemet the **ISnTracer** interface that defines only two simple methods:
```csharp
public interface ISnTracer
{
    void Write(string line);
    void Flush();
}
```
The write method writes a single line message to the provider's target. The Flush method 
prepares the component to the system shutting down, for example empty the internal buffers if it has any. 

Other customization possibility is developing an inherited class from the **BufferedSnTracer** and implementing the required method:
```csharp
protected abstract void WriteBatch(string message);
```
The **BufferedSnTracer** provides base functionality for buffered writing. The main feature of such writer is *less frequent writing* but writing more data in batch. The writes happen periodically, every writing transfers the whole content of the buffer. This is an advantage if writing requires an expensive operation (for example opening a database connection or a file). Buffering may be a disadvantage in case of an accidental system shutdown or killing the current process because the last content of the buffer can be lost. Fortunately, these cases occur quite rarely in modern systems.

The **BufferedSnTracer** needs to be initialized with two important parameters:
```csharp
protected void Initialize(long bufferSize, int writeDelay)
```
The `bufferSize` defines the count of lines that can be buffered. If the buffer is full, the next line overrides the oldest line and the "BUFFER OVERRUN ERROR" message will be written to the log. Default value is 10000. The `writeDelay` is the time between two writing operations in milliseconds. Default value is 1000. If the currently written block size is greater than 20% of the buffer size, a special message will be written in the block (only the maximum values are written), for example:
```text
MaxPdiff: 4289
```

### Built-in trace providers
In this version there are 2 trace provider. One for tracing production webservers with persisted logs and one for monitoring developer's local webserver instances.

### SnFileSystemTracer
This is the default trace provider of sensenet that is inherited from the abstract **BufferedSnTracer**. This trace writer persists the trace data to physical files in the file system. To ensure the velocity this module does the followings:

- Buffered writing: the provider does not write every single line when they come in (see details above).
- After a configured number of lines **new files are opened** to ensure that they do not become too big.
- Files are written locally on every web server instead of a shared drive. We use a very simple data structure.

The files are written into the following directory: `.\App_Data\DetailedLog`. File names contain the UTC creation date of the file: `detailedlog_20160424-144300Z.log`.

This provider's behavior can be fine-tuned by modifying the values of the following configuration items in the web.config:
- **BufferSize**: Allocated line buffer. Default: 10000.
- **WriteToFileDelay**: Time between the end of the previous and start of the next writing in milliseconds. Default: 1000.
- **MaxWritesInOneFile**: Number of writes into a single file. Default: 100. After this a new log file will be created.
```xml
<sensenet>
    <detailedLogger>
      <add key="BufferSize" value="10000" />
      <add key="WriteToFileDelay" value="1000" />
      <add key="MaxWritesInOneFile" value="100" />
    </detailedLogger>
</sensenet>
```

### SnDebugViewTracer
Sometimes - especially developer time - can be useful if trace entries are displayed immediately when they come up. This trace writer transfers the trace entries to the standard trace channel of Windows and the channel can be monitored using the **[DebugView](https://docs.microsoft.com/en-us/sysinternals/downloads/debugview#introduction)** of *[Sysinternals](https://docs.microsoft.com/en-us/sysinternals/)* ([download](https://docs.microsoft.com/en-us/sysinternals/downloads/debugview)). This channel can be very noisy sometimes but can be filtered. Open the filter/highligt dialog from the Edit menu (or press ctrl-L) and set the value of the `Include` textbox to "SnTrace:".

To activate this provider an instance have to be included in the provider-list when the application starts. The following code removes the persistent trace writer and adds the monitoring provider.
```csharp
SnTrace.SnTracers.Clear();
SnTrace.SnTracers.Add(new SnDebugViewTracer());
```

## Performance considerations
There are a couple of things to consider when using SnTrace as a developer or operator:

- Do not keep all categories switched on for a long time (it generates to much data anyway that will be hard to process).
- Do not use complex information gathering when you write to the trace, it may effect time measurement and slow down the portal. If it is necessary, test whether the category (Custom) is enabled or not before collecting the data.
- Delete log files on the server from time to time, they may take up a lot of space.
- Do not open an active file (during measuring) to prevent IO errors.
## Known issues
- Sometimes one or more rows change places in the log during high load. This may happen close to the block start or end.
- Opening an active file may cause an error, but only in the opening application (e.g. Notepad). It does not cause any errors in the trace process.