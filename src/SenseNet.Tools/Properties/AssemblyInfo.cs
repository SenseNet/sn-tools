using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if DEBUG
[assembly: AssemblyTitle("SenseNet.Tools (Debug)")]
#else
[assembly: AssemblyTitle("SenseNet.Tools (Release)")]
#endif

[assembly: AssemblyDescription("Tools library for sensenet ECM")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Sense/Net Inc.")]
[assembly: AssemblyProduct("sensenet ECM")]
[assembly: AssemblyCopyright("Copyright © Sense/Net Inc.")]
[assembly: AssemblyTrademark("Sense/Net Inc.")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]
[assembly: Guid("9c2fa8d5-65f8-4cad-a65d-b7cc804a821f")]

[assembly: AssemblyVersion("2.2.0.0")]
[assembly: AssemblyFileVersion("2.2.0.0")]
[assembly: AssemblyInformationalVersion("2.2.0")]

[assembly: InternalsVisibleTo("SenseNet.Tools.Tests")]
