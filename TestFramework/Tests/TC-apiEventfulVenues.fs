namespace TestFramework

open System
open System.Collections.Generic
open HttpFs.Client
open NUnit.Framework
open TestFramework.Lib
open Helpers
open BingSampleAuthentication


[<TestFixture>]
module ``TC-apiEventfulVenues`` =


    [<TestFixtureSetUp>] //TestFixtureSetUp is executed once 
    let TestSetup() =
        setTestEnvironmentFromConfig()
        System.Net.ServicePointManager.Expect100Continue <- false
        Helpers.addLogEntry ("Info", "Environment", sprintf "%A" TestEnvrionment, "0")
        Helpers.addLogEntry ("Info", "Eventful Test URL:", EventfulUrl(), "0")
        getTestDataForVenuesSearch()
        Helpers.addLogEntry ("Info", "Eventful Venues ID data ssigned", "", "0")
        // In case post new records required:
        // In progress, not done yet.
        //postRequestToken CurrentAuthParams


    [<SetUp>] // SetUp will be executed for every test case
        //If required, using follow function to create Venue data
//        let EachSetup() = 
//            let response = apiEventfulVenuesNew EventfulAppKey null true
//            printfn "Debug: Response code is %A" response.statusCode
//            let body = response |> getBody
//            printfn "Debug: Response code is %A" response.statusCode

    [<Test>]
    let ``TC-EventfulVenuesGet-BVT`` () =
        let queryData = Dictionary<string,string>()
        [
            "id", TestDataVenues
            "mature", "all"
        ] |> Seq.iter queryData.Add

        let response = apiEventfulVenuesGet EventfulAppKey queryData true
        Assert.AreEqual(response.statusCode, 200)
        let body = response |> getBody
        
        let samples = TypeEventfulVenuesGetResultJson.Parse(body)
        let responseId = samples.Id
        Assert.AreEqual(responseId, TestDataVenues)

    [<Test>]
    [<TestCase("", 200, 1)>]
    [<TestCase("*", 200, 1)>]
    [<TestCase("%1=1%", 200, 1)>]
    [<TestCase("null", 200, 1)>]
    [<TestCase("+OR 1=1", 200, 1)>]
    [<TestCase("'+OR 1=1", 200, 1)>]
    let ``TC-EventfulVenuesGet-Security`` (param, expectedResponseCode, expectedErrorCode) =
        let queryData = Dictionary<string,string>()
        [
            "id", TestDataVenues
            "mature", "all"
        ] |> Seq.iter queryData.Add

        let response = apiEventfulVenuesGet param queryData true
        Assert.AreEqual(response.statusCode, 200)
        let body = response |> getBody
        
        let samples = TypeEventfulErrorResultJson.Parse(body)
        let responseId = samples.Error 
        Assert.AreEqual(responseId, expectedErrorCode)
        //Sample: {"status":"Authentication error","error":"1","description":"A valid application key is required."}
        assert(samples.Status = "Authentication error")

    [<Test>]
    [<TestCase("all", 200, 1)>]
    [<TestCase("normal", 200, 1)>]
    [<TestCase("safe", 200, 1)>]
    let ``TC-EventfulVenuesGet-Param-Mature``(param, expectedResponseCode, expectedErrorCode) =
        let queryData = Dictionary<string,string>()
        [
            "id", TestDataVenues
            "mature", param
        ] |> Seq.iter queryData.Add

        let response = apiEventfulVenuesGet EventfulAppKey queryData true
        Assert.AreEqual(response.statusCode, expectedResponseCode)
        let body = response |> getBody
        
        let samples = TypeEventfulVenuesGetResultJson.Parse(body)
        let responseId = samples.Id
        Assert.AreEqual(responseId, TestDataVenues)
        
    [<Test>]
    [<TestCase("", 200, 1)>]
    [<TestCase("This is a wrong Id", 200, 1)>]
    [<TestCase("null", 200, 1)>]
    [<TestCase("*", 200, 1)>]
    [<TestCase("%%", 200, 1)>]
    let ``TC-EventfulVenuesGet-Param-Nagative-Id``(param, expectedResponseCode, expectedErrorCode) =
        let queryData = Dictionary<string,string>()
        [
            "id", param
            "mature", "all"
        ] |> Seq.iter queryData.Add

        let response = apiEventfulVenuesGet EventfulAppKey queryData true
        Assert.AreEqual(response.statusCode, expectedResponseCode)
        let body = response |> getBody
        
        let samples = TypeEventfulErrorResultJson.Parse(body)
        let errorId = samples.Error 
        Assert.AreEqual(errorId, expectedErrorCode)
        
        
    [<Test>]
    [<TestCase("", 200, 1)>]
    [<TestCase("This+is+a+wrong+Mature", 200, 1)>]
    [<TestCase("null", 200, 1)>]
    [<TestCase("*", 200, 1)>]
    [<TestCase("%%", 200, 1)>]
    let ``TC-EventfulVenuesGet-Param-Negative-Mature``(param, expectedResponseCode, expectedErrorCode) =
        let queryData = Dictionary<string,string>()
        [
            "id", TestDataVenues
            "mature", param
        ] |> Seq.iter queryData.Add

        let response = apiEventfulVenuesGet EventfulAppKey queryData true
        Assert.AreEqual(response.statusCode, expectedResponseCode)
        let body = response |> getBody
        
        let samples = TypeEventfulVenuesGetResultJson.Parse(body)
        let responseId = samples.Id
        Assert.AreEqual(responseId, TestDataVenues)


    [<Test>]
    [<TestCase("LocationNotExist", 200, 0)>]
    [<TestCase("\", , \"", 200, 0)>]
    [<TestCase("null", 200, 0)>]
    [<TestCase("\"332.746682, -1317.162741\"", 200, 0)>] //Invalid latitude and longitude
    let ``TC-EventfulSearch-Param-Nagative-Location``(param, expectedResponseCode, expectedQuantity) =
        //Sample: GET http://api.eventful.com/rest/events/search?app_key=Tz3cPsqC8n6w7ghC&keywords=books&location=San+Diego&date=Future HTTP/1.1
        let queryData = Dictionary<string,string>()
        [
            "location", param
            "date", "\"Future\""
            "units", "\"km\""
            "page_size", "10"
            "page_number", "0"
        ] |> Seq.iter queryData.Add

        let response = apiEventfulSearch EventfulAppKey queryData true
        Assert.AreEqual(response.statusCode, expectedResponseCode)
        let body = response |> getBody
        
        let samples = TypeEventfulSearchResultJson.Parse(body)
        Assert.GreaterOrEqual(samples.TotalItems, expectedQuantity)