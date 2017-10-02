namespace TestFramework.AssemblyInfo

open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

[<assembly: AssemblyTitle("API Automation Project")>]
[<assembly: AssemblyDescription("API Automation Project")>]
[<assembly: AssemblyCompany("ABC Corp.")>]
[<assembly: AssemblyProduct("Automation")>]
[<assembly: AssemblyCopyright("Copyright (c) 2017. ABC Corp.")>]

[<assembly: AssemblyVersion("1.0.0.0")>]
[<assembly: AssemblyFileVersion("1.0.0.0")>]
[<assembly: AssemblyInformationalVersion("1.0.0")>]

#if PORTABLE
#else
[<assembly: ComVisible(false)>]
#endif

do
    ()