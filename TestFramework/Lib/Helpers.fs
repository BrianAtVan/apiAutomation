namespace TestFramework.Lib

open System
open System.Collections.Generic
open System.IO
open System.Reflection
open System.Text
open System.Threading
open System.Web
open HttpFs.Client
open NUnit.Framework
open Newtonsoft.Json



[<AutoOpen>]
module Helpers = 
    /// <summary>Create a random ID between 1000 and 9999. Useful for use as USER ID<summary>
    let private randGen = Random();

    let getRandomId() = randGen.Next(1000, 10000)
    let getGuid () = Guid.NewGuid().ToString()
    
    let LogTurnOn = true
    let logFilePath = @"..\" + System.DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".txt"
    
    let addLogEntry (operation, logString, responseSize, timeDifferences) = 
        if LogTurnOn then 
            let textString = 
                System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + "||" + operation + "||" + logString + "||" + responseSize + "||" 
                + timeDifferences + "\r\n"
            //System.IO.File.AppendAllText (logFilePath, textString) |> ignore
            use file = File.Open(logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)
            let data = Encoding.UTF8.GetBytes(textString) //Environment.NewLine)
            file.Write(data, 0, data.Length)

    

    type NameValuePair = 
        { name : string
          value : string }
    
    let customHeader (x : NameValuePair) = Custom(x.name, x.value)
    let customCookie (x : NameValuePair) = Cookie.create (x.name, x.value)
    let jsonContentTypeHeader = ContentType(ContentType.create ("application", "json", Encoding.UTF8))
    let formUrlEncodedContentTypeHeader = ContentType(ContentType.create ("application", "x-www-form-urlencoded", Encoding.UTF8))

    let mutable TestDataVenues = ""

    let genRandomNumbers count = 
        let rnd = System.Random()
        List.init count (fun _ -> rnd.Next())



    let fileDataContentTypeHeader() =
        let boundaryVal = "-Boundary-" + randGen.Next(0, Int32.MaxValue).ToString("x8")
        let ct = ContentType.create("multipart", "form-data", Encoding.UTF8, boundaryVal)
        ContentType ct

    let getResponse request = 
        request
        |> HttpFs.Client.getResponse
        |> Hopac.Hopac.run
    
    let getBody response = 
        response
        |> Response.readBodyAsString
        |> Hopac.Hopac.run
    
    /// <summary>Retrieve a full web page for a given URL.</summary>
    /// <param name="url">Fully qualified URL.</param>
    /// <returns>The HTML page content</returns>
    let readWebPage url = 
        Request.createUrl Get url
        |> getResponse
        |> getBody
    
    /// <summary>Retries an actions few times while the passed function returns false.</summary>
    /// <param name="retryCount">The number of times to retry before giving up</param>
    /// <param name="retryDelay">Number of seconds to wait between retries</param>
    /// <param name="f">A function that returns a boolean result (true/false)</param>
    /// <returns>True if the function returned true in any iteration, false if it returned false in all iterations.</returns>
    /// <remarks>This function will exit as soon as the function returns true and will not go over all iterations.</remarks>
    let retryWithWait retryCount retryDelay f = 
        let rec retry times = 
            if times <= 0 then false
            else if f() then true
            else 
                match box retryDelay with
                | :? int as i when i > 0 -> Thread.Sleep i
                | :? TimeSpan as t when t.TotalSeconds > 0. -> Thread.Sleep t
                | _ -> () // ignore delay
                retry (times - 1)
        retry retryCount
    
    /// <summary>Retries an actions few times while the passed function returns false.</summary>
    /// <param name="retryCount">The number of times to retry before giving up</param>
    /// <param name="f">A function that returns a boolean result (true/false)</param>
    /// <returns>True if the function returned true in any iteration, false if it returned false in all iterations</returns>
    /// <remarks>This function will exit as soon as the function returns true and will not go over all iterations.</remarks>
    let retryWithNoWait retryCount f = retryWithWait retryCount 0 f
    
    /// <summary>ries to convert an item to an instance of another.</summary>
    /// <param name="convertible">An objec to try casting into another</param>
    /// <returns>An Option of the converted object (Some or None)</returns>
    /// <remarks>This can be used by tests to validate whether returned values are assignable from one type to another.</remarks>
    let convertTo<'a> convertible = 
        match box convertible with
        | :? 'a -> Some convertible
        | _ -> None
    
    /// <summary>
    /// Forms a full Uri for an url nad 
    /// </summary>
    /// <param name="url">The HTTP/HTTP url for the service</param>
    /// <param name="resource">The resource within the service</param>
    let uriFor (url : string) (resource : string) = 
        let uri = Uri(sprintf "%s/%s" (url.TrimEnd('/')) (resource.TrimStart('/')))
        uri.AbsoluteUri
    
    /// <summary>
    /// Adds few default headers to the WEB API request.
    /// </summary>
    /// <param name="request">An HTTP Client request object.</param>
    let withDefaultSettings (request : Request) = 
        request
        |> Request.keepAlive false
        |> Request.responseCharacterEncoding Encoding.UTF8
        |> Request.autoDecompression DecompressionScheme.GZip
        |> Request.setHeader (Accept "application/json")
        |> Request.setHeader (UserAgent DefaultUserAgent)
    
    let withFormBody (bodyContent : Dictionary<string, string>) (request : Request) = 
        let formData = 
            bodyContent 
            |> Seq.map (fun keyValuePair -> sprintf "%s=%s" (HttpUtility.UrlEncode(keyValuePair.Key)) (HttpUtility.UrlEncode(keyValuePair.Value)))
        let formEndodedString = String.Join("&", formData)
        request
        |> Request.setHeader formUrlEncodedContentTypeHeader
        |> Request.bodyStringEncoded formEndodedString Encoding.UTF8
    
    let withJsonBody bodyContent (request : Request) = 
        let body = 
            match box bodyContent with
            | :? String as s -> s
            | _ -> JsonConvert.SerializeObject bodyContent
        request
        |> Request.setHeader jsonContentTypeHeader
        |> Request.bodyStringEncoded body Encoding.UTF8
    
    let getFrom uri = Request.createUrl Get uri |> withDefaultSettings
    let postTo uri = Request.createUrl Post uri |> withDefaultSettings
    let delete uri = Request.createUrl Delete uri |> withDefaultSettings
    let putTo uri = Request.createUrl Put uri |> withDefaultSettings

    let exeLocation =
        let p = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
        let d = DirectoryInfo(p)
        d.FullName

    let apiEventfulVenuesGet (appKey:string) (queryDict:Dictionary<string, string>) (skipLog:bool) = 
        let startTime = System.DateTime.Now.TimeOfDay
        //Debug: printfn "queryDictKeys %A" queryDict.Keys
        let mutable queryString = ""
        for pair in queryDict do
            queryString <- queryString + "&" + pair.Key + "=" + pair.Value

        if queryString = "" then queryString <- "&keywords=Restaurant&location=San+Diego"

        let request =
            getFrom (EventfulUrl() + "/venues/get?app_key="+appKey+ queryString)
        let response = request |> getResponse
        addLogEntry ("GET", request.url.ToString(), response.statusCode.ToString(), (System.DateTime.Now.TimeOfDay - startTime).ToString())
        response

    let apiEventfulVenuesSearch (appKey:string) (queryDict:Dictionary<string, string>) (skipLog:bool) = 
        let startTime = System.DateTime.Now.TimeOfDay
        //Debug: printfn "queryDictKeys %A" queryDict.Keys
        let mutable queryString = ""
        for pair in queryDict do
            queryString <- queryString + "&" + pair.Key + "=" + pair.Value

        if queryString = "" then queryString <- "&keywords=Restaurant&location=San+Diego"

        let request =
            getFrom (EventfulUrl() + "/venues/search?app_key="+appKey+ queryString)
        let response = request |> getResponse
        addLogEntry ("GET", request.url.ToString(), response.statusCode.ToString(), (System.DateTime.Now.TimeOfDay - startTime).ToString())
        response

    let getTestDataForVenuesSearch() =
        TestDataVenues <- ""
        let queryData = Dictionary<string,string>()
        [
            "location", "San+Diego"
            "keywords", "Restaurant"
        ] |> Seq.iter queryData.Add

        let response = apiEventfulVenuesSearch EventfulAppKey queryData true
        Assert.AreEqual(response.statusCode, 200)
        let body = response |> getBody
        let samples = TypeEventfulVenuesSearchResultJson.Parse(body)
        TestDataVenues <- samples.Venues.Venue.[0].Id.ToString()


    let apiEventfulSearch (appKey:string) (queryDict:Dictionary<string, string>) (skipLog:bool) = 
        let startTime = System.DateTime.Now.TimeOfDay
        //Debug: printfn "queryDictKeys %A" queryDict.Keys
        let mutable queryString = ""
        for pair in queryDict do
            queryString <- queryString + "&" + pair.Key + "=" + pair.Value

        if queryString = "" then queryString <- "&keywords=books&location=San+Diego&date=Future"

        let request =
            getFrom (EventfulUrl() + "/events/search?app_key="+appKey+ queryString)
        let response = request |> getResponse
        addLogEntry ("GET", request.url.ToString(), response.statusCode.ToString(), (System.DateTime.Now.TimeOfDay - startTime).ToString())
        response

    let apiEventfulVenuesNew (appKey:string) (queryDict:Dictionary<string, string>) (skipLog:bool) = 
        let startTime = System.DateTime.Now.TimeOfDay
        //Debug: printfn "queryDictKeys %A" queryDict.Keys
        let mutable queryString = ""
        if not (isNull(queryDict)) then 
            for pair in queryDict do
                queryString <- queryString + "&" + pair.Key + "=" + pair.Value
        
        //if queryString is empty, take default one.
        if queryString = "" then queryString <- "&name=BingVenue"+getGuid()+"&city=Vancouver&country=Canada&venue_type=Cinema"

        //0779ad19e3ec705b4aeb
        let request =
            postTo (EventfulUrl() + "/venues/new?app_key="+appKey+"&oauth_consumer_key="+"0779ad19e3ec705b4aeb"+queryString)
        let response = request |> getResponse
        addLogEntry ("POST", request.url.ToString(), response.statusCode.ToString(), (System.DateTime.Now.TimeOfDay - startTime).ToString())
        response

    let apiEventfulEventNew (appKey:string, queryDict:Dictionary<string, string>, skipLog:bool) = 
        let startTime = System.DateTime.Now.TimeOfDay
        //Debug: printfn "queryDictKeys %A" queryDict.Keys
        let mutable queryString = ""
        for pair in queryDict do
            queryString <- queryString + "&" + pair.Key + "=" + pair.Value

        if queryString = "" then queryString <- "&title=BingEvent"+getGuid()+"&start_time=2017-10-04+17:00:00&venue_id=San+Diego&date=Future"

        let request =
            getFrom (EventfulUrl() + "/events/search?app_key="+appKey+ queryString)
        let response = request |> getResponse
        addLogEntry ("GET", request.url.ToString(), response.statusCode.ToString(), (System.DateTime.Now.TimeOfDay - startTime).ToString())
        response

