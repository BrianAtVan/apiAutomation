(* ======================================================
Use F# to run unit tests without having to install NUnit
From command line run:
  fsi --noframework Script.fsx 

In my system, fsi.exe is located here:
  C:\Program Files (x86)\Microsoft SDKs\F#\4.0\Framework\v4.0\Fsi.exe

From the article "Low risk ways to use F# at work"
  http://fsharpforfunandprofit.com/posts/low-risk-ways-to-use-fsharp-at-work/
====================================================== *)

// sets the current directory to be same as the script directory
System.IO.Directory.SetCurrentDirectory (__SOURCE_DIRECTORY__)

// Requires Nunit.Runners under script directory 
//    nuget install NUnit.Runners -o Packages -ExcludeVersion 

//#r @"..\Packages\FSharp.Core.4.0.0.1\lib\net40\FSharp.Core.dll"
#r @"..\Packages\NUnit.Runners.2.6.4\tools\lib\nunit.core.dll"
#r @"..\Packages\NUnit.Runners.2.6.4\tools\lib\nunit.core.interfaces.dll"

open System
open NUnit.Core

module Setup = 
    open System.Reflection
    open NUnit.Core
    open System.Diagnostics.Tracing

    let configureTestRunner path (runner:TestRunner) = 
        let package = TestPackage("TestFramework")
        package.Assemblies.Add(path) |> ignore
        runner.Load(package) |> ignore

    let createListener logger =

        let replaceNewline (s:string) = 
            s.Replace(Environment.NewLine, "")

        // This is an example of F#'s "object expression" syntax.
        // You don't need to create a class to implement an interface
        {new NUnit.Core.EventListener
            with
        
            member this.RunStarted(name:string, testCount:int) =
                logger "Run started "

            member this.RunFinished(result:TestResult ) = 
                logger ""
                logger "-------------------------------"
                result.ResultState
                |> sprintf "Overall result: %O" 
                |> logger 

            member this.RunFinished(ex:Exception) = 
                ex.StackTrace 
                |> replaceNewline 
                |> sprintf "Exception occurred: %s" 
                |> logger 

            member this.SuiteFinished(result:TestResult) = ()
            member this.SuiteStarted(testName:TestName) = ()

            member this.TestFinished(result:TestResult)=
                result.ResultState
                |> sprintf "Result: %O" 
                |> logger 

            member this.TestOutput(testOutput:TestOutput) = 
                testOutput.Text 
                |> replaceNewline 
                |> logger 

            member this.TestStarted(testName:TestName) = 
                logger ""
            
                testName.FullName 
                |> replaceNewline 
                |> logger 

            member this.UnhandledException(ex:Exception) = 
                ex.StackTrace 
                |> replaceNewline 
                |> sprintf "Unhandled exception occurred: %s"
                |> logger 
            }

// run all the tests in the DLL
do 
    let dllPath = @".\bin\debug\TestFramework.dll"
    //let dllPath = @".\bin\release\TestFramework.dll"

    CoreExtensions.Host.InitializeService();

    use runner = new NUnit.Core.SimpleTestRunner()
    Setup.configureTestRunner dllPath runner
    let logger = printfn "%s"
    let listener = Setup.createListener logger
    let result = runner.Run(listener, TestFilter.Empty, true, LoggingThreshold.All)

    // if running from the command line, wait for user input
    Console.ReadLine() |> ignore
