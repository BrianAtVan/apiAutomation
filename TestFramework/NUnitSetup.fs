namespace TestFramework

open System
open NUnit.Core

module NUnitSetup =

    System.IO.Directory.SetCurrentDirectory (__SOURCE_DIRECTORY__)

    let configureTestRunner path (runner:TestRunner) = 
        let package = TestPackage("TestFramework")
        package.Assemblies.Add(path) |> ignore
        runner.Load(package) |> ignore

    let createListener logger =
        let replaceNewline (s:string) = 
            s.Replace(Environment.NewLine, "")

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
