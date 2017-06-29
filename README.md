# sensenet ECM Tools

[![Join the chat at https://gitter.im/SenseNet/sn-tools](https://badges.gitter.im/SenseNet/sn-tools.svg)](https://gitter.im/SenseNet/sn-tools?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![NuGet](https://img.shields.io/nuget/v/SenseNet.Tools.svg)](https://www.nuget.org/packages/SenseNet.Tools)

This library contains useful tools for developers ranging from tasks as small as retrying an operation multiple times to a robust trace component.

The library is **independent from sensenet ECM** and it does not have any sensenet ECM-related dependencies. It is the other way around: [sensenet ECM](https://github.com/SenseNet/sensenet) relies heavily on the tools published in this library.

You can even use it in your custom project that has nothing to do with Sense/Net ECM!

This is a constantly evolving component, we plan to add new features to it as needed. Feel free to contribute or make suggestions on how to improve it!

## Retrier
This is a lightweight but powerful **API for retrying operations** that should be executed even if some kind of an exception is thrown for the first time.

If no error occurs, the operation will be executed only once of course. After retrying for the given number of times - if the error still occurs - the exception will be thrown for the caller to catch. 

Only trhe provided exception type is monitored and suppressed. All other exceptions are thrown immediately.

````csharp
// retry something maximum 3 times, waiting 10 milliseconds in between
Retrier.Retry(3, 10, typeof(InvalidOperationException), () =>
{
   // execute something that may throw an invalid operation exception
   DoSomething();
});

// retrying an async operation with return value
var result = await Retrier.RetryAsync<int>(3, 10, async () =>
{
   return await DoSomethingAsync();
},
(r, i, e) => e == null);
````

## Diagnostics
In this namespace you'll find easy-to-use and extendable tracing and logging components. We build on them extensively in the core sensenet ECM project but they can also be used in any kind of tool or application as a lightweight logging technology.

```csharp
SnLog.WriteInformation("ContentTypeManager loaded.");
```

See details [here](src/SenseNet.Tools/Diagnostics/Readme.md).

## Command line arguments
The classes in this namespace provide an easy way for developers to create **command line tools** that can be invoked with rich command line arguments.

See details [here](src/SenseNet.Tools/Tools/CommandLineArguments/Readme.md).

## Asynchronous ForEach
This API allows you to **execute an async operation on a list in parallel**, with defining the maximum number of parallel operations. This feature is currently missing from the .Net Framework TPL/PLINQ and is useful when you have to execute a large number of operations but have to prevent resource overload - for example when calling a web service.
````csharp
await myList.ForEachAsync(parallelCount, async i =>
{
   await DoSomethingAsync();
})
````

## TypeResolver
This is a simple API for loading types from the current app domain or a custom execution directory and creating object instances. Loaded types are cached and can be used in an IoC/DI scenario, or when working with pinned object instances.
````csharp
var types = TypeResolver.GetTypesByInterface(typeof(ICustomInterface));
var dbProvider = TypeResolver.CreateInstance<DbProvider>("MyNamespace.MyDbProvider");
````

## Configuration
This is a simple base API for loading **strongly typed values** from .Net configuration files. It lets you define your custom config classes and publish config properties with only a few lines of code.

See details [here](src/SenseNet.Tools/Configuration/Readme.md).
