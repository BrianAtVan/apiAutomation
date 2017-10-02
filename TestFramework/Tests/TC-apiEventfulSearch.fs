namespace TestFramework

open System
open HttpFs.Client
open NUnit.Framework
open TestFramework.Lib
open Helpers
open System.Collections.Generic

[<TestFixture>]
module ``TC-apiEventfulSearch`` =


    let [<Literal>] StrSearchBVTQuery = """""|"Vancouver"|"All"|""|0|"km"|false|"date"|"ascending"|10|0|""|0|""|""|false"""

    [<TestFixtureSetUp>] //TestFixtureSetUp is executed once 
    let TestSetup() =
        setTestEnvironmentFromConfig()
        System.Net.ServicePointManager.Expect100Continue <- false
        Helpers.addLogEntry ("Info", "Environment", sprintf "%A" TestEnvrionment, "0")
        Helpers.addLogEntry ("Info", "Eventful Test URL:", EventfulUrl(), "0")
        
        Helpers.addLogEntry ("Info", "Eventful Test BVT Query Data assigned", "", "0")
        
    [<SetUp>] // SetUp will be executed for every test case

    [<Test>]
    let ``TC-EventfulSearch-BVT`` () =
        //Sample: GET http://api.eventful.com/rest/events/search?app_key=Tz3cPsqC8n6w7ghC&keywords=books&location=San+Diego&date=Future HTTP/1.1
        let queryData = Dictionary<string,string>()
        [
            "location", "\"Vancouver\""
            "date", "\"Past\""
            "units", "\"km\""
            "page_size", "10"
            "page_number", "0"
        ] |> Seq.iter queryData.Add

        let response = apiEventfulSearch EventfulAppKey queryData true
        Assert.AreEqual(response.statusCode, 200)
        let body = response |> getBody
        
        let samples = TypeEventfulSearchResultJson.Parse(body)
        Assert.Greater(samples.TotalItems, 100)
        
    [<Test>]
    [<TestCase("Future", 200, 100)>]
    [<TestCase("Past", 200, 100)>]
    [<TestCase("2013042500-2017042700", 200, 100)>]
    [<TestCase("Today", 200, 0)>]
    [<TestCase("Last Week", 200, 100)>]
    [<TestCase("October", 200, 100)>]
    let ``TC-EventfulSearch-Param-Date``(param, expectedResponseCode, expectedQuantity) =
        //Sample: GET http://api.eventful.com/rest/events/search?app_key=Tz3cPsqC8n6w7ghC&keywords=books&location=San+Diego&date=Future HTTP/1.1
        let queryData = Dictionary<string,string>()
        [
            "location", "Vancouver"
            "date", param
            "units", "\"km\""
            "page_size", "10"
            "page_number", "0"
        ] |> Seq.iter queryData.Add

        let response = apiEventfulSearch EventfulAppKey queryData true
        Assert.AreEqual(response.statusCode, expectedResponseCode)
        let body = response |> getBody
        
        let samples = TypeEventfulSearchResultJson.Parse(body)
        Assert.GreaterOrEqual(samples.TotalItems, expectedQuantity)
        
        
    [<Test>]
    [<TestCase("Vancouver", 200, 100)>]
    [<TestCase("\"Vancouver, BC\"", 200, 100)>]
    [<TestCase("\"Vancouver, BC, Canada\"", 200, 100)>]
    [<TestCase("Canada", 200, 100)>]
    [<TestCase("32.746682, -117.162741", 200, 100)>]
    [<TestCase("\"\"", 200, 0)>]
    let ``TC-EventfulSearch-Param-Location``(param, expectedResponseCode, expectedQuantity) =
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