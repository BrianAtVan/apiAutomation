namespace TestFramework

open System
open System.IO
open System.Text
open System.Collections.Generic
open HttpFs.Client
open TestFramework.Lib
open StatusCodes
open Licensing
open FsUnit

module BingSampleAuthentication = 
    type LogInfo = 
        | Operation of string // Get, Put, Post
        | LogString of string
        | RequestSize of string
        | ResponseSize of string
        | TimeDifferences of string
    
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
    
    type public ClientIDandSecret = 
        { mutable ClientId : string
          mutable ClientSecret : string }
    
    type public InstanceParams = 
        { mutable ServiceId : string
          mutable HostGuid : Guid
          mutable License : string
          mutable ProductName : string
          mutable ProductVersion : string
          mutable TabModuleId : string }
    
    let mutable public CurrentSCClientIDandSecret = 
        { ClientId = ""
          ClientSecret = "" }
    
    let mutable public CurrentFBClientIDandSecret = 
        { ClientId = ""
          ClientSecret = "" }
    
    let mutable public CurrentInstanceParams = 
        { ServiceId = "todolist"
          HostGuid = Guid.Empty
          License = "CLOUD-SOC-INTERNAL-2014-1"
          ProductName = "DNNCORP.F#.Automation"
          ProductVersion = "1.0"
          TabModuleId = "" }
    
    let getTokenRenew existClientId existClientSecret existClientData = 
        let request = 
            postTo (TokenServiceUrl())
            |> Request.basicAuthentication existClientId existClientSecret
            |> withFormBody existClientData
        
        let response = request |> getResponse
        if response.statusCode <> ok then 
            //printfn "\r\nREQUEST =>\r\n%A\r\n" request
            failwith "Authentication::Token Failed to renew. "
        //printfn "\r\nRESPONSE =>\r\n%A\r\n" response
        else 
            let content = response |> getBody
            let myToken = MsaToken.Parse content
            let myReturn = myToken.AccessToken
            //printfn "\r\nTOKEN =>\r\n%A\r\n" myReturn
            myReturn
    
    //let getCurrentClientIDandSecret =
    //    (CurrentClientIDandSecret.ClientId, CurrentClientIDandSecret.ClientSecret)
    let setCurrentClientIDandSecret serviceId clientId clientSecret = 
        if serviceId = StructuredContentServiceId then 
            CurrentSCClientIDandSecret.ClientId <- clientId
            CurrentSCClientIDandSecret.ClientSecret <- clientSecret
        else 
            CurrentFBClientIDandSecret.ClientId <- clientId
            CurrentFBClientIDandSecret.ClientSecret <- clientSecret
        ()
    
    let setFBClientIDandSecret clientId clientSecret = 
        CurrentFBClientIDandSecret.ClientId <- clientId
        CurrentFBClientIDandSecret.ClientSecret <- clientSecret
        ()
    
    let setCurrentInstanceParams serviceID hostGuid = 
        CurrentInstanceParams.HostGuid <- hostGuid
        CurrentInstanceParams.ServiceId <- serviceID
        ()
    
    let getTokenForSameClient (serviceID : string) (currentClientData : Dictionary<string, string>) = 
        //let instanceParams = defaultActivationParameters
        //let clientId, clientSecret = getCurrentClientIDandSecret
        //currentClientData.Item("scope") <- permissions
        let mutable currentClientId = ""
        let mutable currentClientSecret = ""
        if serviceID = StructuredContentServiceId then 
            currentClientId <- CurrentSCClientIDandSecret.ClientId
            currentClientSecret <- CurrentSCClientIDandSecret.ClientSecret
        else 
            currentClientId <- CurrentSCClientIDandSecret.ClientId
            currentClientSecret <- CurrentSCClientIDandSecret.ClientSecret
        let request = 
            postTo (TokenServiceUrl())
            |> Request.basicAuthentication currentClientId currentClientSecret
            |> withFormBody currentClientData
        
        let response = request |> getResponse
        if response.statusCode <> ok then 
            //printfn "\r\nREQUEST =>\r\n%A\r\n" request
            failwith "Authentication::Token Failed to retrieve. "
            (null, currentClientId, currentClientSecret, currentClientData)
        //printfn "\r\nRESPONSE =>\r\n%A\r\n" response
        else 
            let content = response |> getBody
            let myToken = MsaToken.Parse content
            let myReturn = myToken.AccessToken
            (//printfn "\r\nTOKEN =>\r\n%A\r\n" myReturn
             myReturn, currentClientId, currentClientSecret, currentClientData)
    
    //Public Function to getToken
    //Parameter to provide:
    // - ServiceID: To Identify which Micro Service. For instance: Form Builder or Structured Content
    // - Permissions: A String combination of Permission Scope. For instance: "sc-type:read sc-type:write"
    // - expectedClientId: This is a pre-defined clientId based on Test license account. 
    let getToken (hostId:Guid, serviceId:string, permissions:string, expectedClientId:string) = 
        let myToken = MsaToken.GetSample
        let myReturn = ""
        let startTime = System.DateTime.Now.TimeOfDay
        
        let myHostId = hostId
        let instanceParams = 
            { defaultActivationParameters with ServiceId = serviceId
                                               HostGuid = myHostId }
        //addLogEntry ("Auth", "ServiceId", instanceParams.ServiceId.ToString(), (System.DateTime.Now.TimeOfDay - startTime).ToString())
        addLogEntry ("Auth", "HostGuid::ServiceId", instanceParams.HostGuid.ToString() + "::" + instanceParams.ServiceId.ToString(), "0")
        let clientId, clientSecret = instanceParams |> getClientIdAndSecret (LicensingServiceUrl())
        addLogEntry ("Auth", "clientId::clientSecret", clientId + "::" + clientSecret, (System.DateTime.Now.TimeOfDay - startTime).ToString())
        if isNull clientId then (null, null, null, null)
        else 
            //test <@ clientId = expectedClientId @>
            Assert.NotNull clientSecret
            setCurrentClientIDandSecret serviceId clientId clientSecret
            setCurrentInstanceParams instanceParams.ServiceId instanceParams.HostGuid
            let clientData = Dictionary<string, string>()
            [ "grant_type", "dnncustom"
              "scope", permissions
              "prodname", instanceParams.ProductName
              "prodver", instanceParams.ProductVersion
              "hostid", instanceParams.HostGuid.ToString()
              "portalid", "0"
              "userid", "1"
              "username", "host"
              "environment", "Production"
              "clienttenantid", instanceParams.HostGuid.ToString() ]
            |> Seq.iter clientData.Add
            let request = 
                postTo (TokenServiceUrl())
                |> Request.basicAuthentication clientId clientSecret
                |> withFormBody clientData
            
            let response = request |> getResponse
            addLogEntry ("Auth", "Auth Done", response.contentLength.ToString(), (System.DateTime.Now.TimeOfDay - startTime).ToString())
            if response.statusCode <> ok then 
                //printfn "\r\nREQUEST =>\r\n%A\r\n" request
                failwith "Authentication::Token Failed to retrieve. "
                (null, null, null, null)
            //printfn "\r\nRESPONSE =>\r\n%A\r\n" response
            else 
                let content = response |> getBody
                let myToken = MsaToken.Parse content
                let myReturn = myToken.AccessToken
                addLogEntry ("Auth", "Token Reference", myReturn.ToString(), "0")
                //printfn "\r\nTOKEN =>\r\n%A\r\n" myReturn
                (myReturn, clientId, clientSecret, clientData)

//    let getLanguageToken  = 
//        let request = 
//                postTo (TokenServiceUrl())
//                |> Request.basicAuthentication clientId clientSecret
//                |> withFormBody clientData
//        let response = request |> getResponse
//        let content = response |> getBody


(*

    [<Test>]
    let ``API-GetStructuredContent-Token`` () =        
        let myToken = getToken StructuredContentServiceId StructuredContentRead StructuredContentClientId
        test <@ myToken.Length > 1 @>
        let SC_Token = myToken
        
        let request = 
            getFrom (StructuredContentUrl + "/api/ContentTypes?startindex=0")
            |> Request.setHeader (Authorization("Bearer " + myToken))
        printfn "\r\nREQUEST =>\r\n%A\r\n" request
        let response = request |> getResponse
        test <@ response.statusCode = 200 @> 
        response |> getBody |> should contain "totalResultCount"

    [<Test>]
    let ``API-GetFormBuilder-Token`` () =        
        let myToken = getToken FormBuilderServiceId FormBuilderRead FormBuilderClientId
        test <@ myToken.Length > 1 @>
        let FB_Token = myToken
        let request = 
            getFrom (StructuredContentUrl + "/api/ContentTypes?startindex=0")
            |> Request.setHeader (Authorization("Bearer " + myToken))
        printfn "\r\nREQUEST =>\r\n%A\r\n" request
        let response = request |> getResponse
        test <@ response.statusCode = 200 @> 
        response |> getBody |> should contain "totalResultCount"

    [<Test>]
    let ``API-GetStructuredContent-TokenwithWrongClientId`` () =        
        
        let myToken = getToken "" StructuredContentRead StructuredContentClientId
        
        test <@ myToken = null @>
        let SC_Token = myToken
        
        let request = 
            getFrom (StructuredContentUrl + "/api/ContentTypes?startindex=0")
            |> Request.setHeader (Authorization("Bearer " + myToken))
        printfn "\r\nREQUEST =>\r\n%A\r\n" request
        let response = request |> getResponse
        test <@ response.statusCode = 200 @> 
        response |> getBody |> should contain "totalResultCount"
        *)

(*

    [<TestCase(FormBuilderServiceId, FormBuilderClientId)>]
    [<TestCase(StructuredContentServiceId, StructuredContentClientId)>]
    let ``Register and Activate instance should return valid client ID and secret``(serviceId, expectedClientId) = 
        let instanceParams = { defaultActivationParameters with ServiceId = serviceId; HostGuid = Guid.NewGuid() }

        let cert, response = instanceParams |> registerInstance LicensingServiceUrl
        test <@ response.statusCode = ok @>
        Assert.NotNull cert

        let activateResult, error = instanceParams |> activateInstance LicensingServiceUrl cert
        Assert.IsNotNull activateResult
        Assert.IsNull error
        test <@ activateResult.ClientId = expectedClientId @>
        //test <@ activateResult.ClientSecret = clientSecret @> -- this varies from HostGuid to another

    [<TestCase(FormBuilderServiceId, FormBuilderRead, FormBuilderClientId)>]
    [<TestCase(StructuredContentServiceId, StructuredContentRead, StructuredContentClientId)>]
    let ``Testing Get Token``(serviceId, permissions, expectedClientId) = 
        let instanceParams = { defaultActivationParameters with ServiceId = serviceId; HostGuid = Guid.NewGuid() }
        let clientId, clientSecret = instanceParams |> getClientIdAndSecret LicensingServiceUrl

        test <@ clientId = expectedClientId @>
        Assert.NotNull clientSecret
        
        let clientData = Dictionary<string,string>()
        [
            "grant_type", "dnncustom"
            "scope", permissions
            "prodname", instanceParams.ProductName
            "prodver", instanceParams.ProductVersion
            "hostid", instanceParams.HostGuid.ToString()
            "portalid", "0"
            "userid", "3"
            "username", "cm"
          ]
          |> Seq.iter clientData.Add
          
        let request = 
            postTo DevTokenServiceUrl
            |> Request.basicAuthentication clientId clientSecret
            |> withFormBody clientData

        let response = request |> getResponse
        if response.statusCode <> ok then
            printfn "\r\nREQUEST =>\r\n%A\r\n" request
            //printfn "\r\nRESPONSE =>\r\n%A\r\n" response
        else
            let content = response |> getBody
            let token = MsaToken.Parse content
            printfn "\r\nTOKEN =>\r\n%A\r\n" token.AccessToken
        test <@ response.statusCode = ok @>

    *)
