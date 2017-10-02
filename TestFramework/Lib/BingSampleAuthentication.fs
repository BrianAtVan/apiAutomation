namespace TestFramework.Lib

open System
open System.IO
open System.Text
open System.Collections.Generic
open HttpFs.Client
open Helpers
open System.Security.Cryptography.Xml
open OAuth10Helpers
open OAuth10
open OAuth10Api

//[<AutoOpen>]
module BingSampleAuthentication = 

    type public AuthParams = 
        { mutable Key : string
          mutable oAuthConsumerKey : string
          mutable oAuthConsumerSecret : string
          mutable oAuthCallback :string
        }

    let mutable public CurrentAuthParams = 
        { Key = "Tz3cPsqC8n6w7ghC"
          oAuthConsumerKey = "0779ad19e3ec705b4aeb"
          oAuthConsumerSecret = "d52275d7b68283aee638"
          oAuthCallback = "http://winwest.com/callback"
        }
    

    let consumerInfo = { consumerKey="0779ad19e3ec705b4aeb";
                         consumerSecret="d52275d7b68283aee638";
                         consumerCallback="http://www.winwest.com/callback"}

    let requestTokenResponse = OAuth10Api.getRequestToken
                                <| Requirement (System.Text.Encoding.ASCII,
                                                EventfulRequestTokenUrl(),
                                                POST)
                                <| consumerInfo
                                <| []

    printfn "requestTokenResponse is %A" requestTokenResponse


    let postRequestToken(currentOAuthData : AuthParams) = 
        let requestTimeStamp =DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()
        //let requestStr = "?app_key=Tz3cPsqC8n6w7ghC&oauth_callback=http%3A%2F%2Fwinwest.com%2Fcallback&oauth_consumer_key=0779ad19e3ec705b4aeb&oauth_consumer_secret=d52275d7b68283aee638&oauth_nonce=1cdb7f498ba9811513f3&oauth_signature_method=HMAC-SHA1&oauth_timestamp="+requestTimeStamp.ToString()
        let requestStr = "?oauth_callback=http%3A%2F%2Fwinwest.com%2Fcallback&oauth_consumer_key=0779ad19e3ec705b4aeb&oauth_nonce=215926984&oauth_signature_method=HMAC-SHA1&oauth_timestamp=1506905956&oauth_version=1.0&oauth_signature=39ndSk%2FCGlNS2%2BIpvxSlK0P%2B7YQ%3D"
        let request = 
            postTo (EventfulRequestTokenUrl()+requestStr)
        let response = request |> getResponse
        printfn "debug: response code is %A " response.statusCode
        let body = response |> getBody
        printfn "debug: response body is %A " body
        

//
//
//    type LogInfo = 
//        | Operation of string // Get, Put, Post
//        | LogString of string
//        | RequestSize of string
//        | ResponseSize of string
//        | TimeDifferences of string
//    
//    
//    type public ClientIDandSecret = 
//        { mutable ClientId : string
//          mutable ClientSecret : string }
//    
//    type public InstanceParams = 
//        { mutable ServiceId : string
//          mutable HostGuid : Guid
//          mutable License : string
//          mutable ProductName : string
//          mutable ProductVersion : string
//          mutable TabModuleId : string }
//    
//    let mutable public CurrentSCClientIDandSecret = 
//        { ClientId = ""
//          ClientSecret = "" }
//    
//    let mutable public CurrentFBClientIDandSecret = 
//        { ClientId = ""
//          ClientSecret = "" }
//    
//    let mutable public CurrentInstanceParams = 
//        { ServiceId = "todolist"
//          HostGuid = Guid.Empty
//          License = "CLOUD-SOC-INTERNAL-2014-1"
//          ProductName = "DNNCORP.F#.Automation"
//          ProductVersion = "1.0"
//          TabModuleId = "" }
//    
//    let getTokenRenew existClientId existClientSecret existClientData = 
//        let request = 
//            postTo (TokenServiceUrl())
//            |> Request.basicAuthentication existClientId existClientSecret
//            |> withFormBody existClientData
//        
//        let response = request |> getResponse
//        if response.statusCode <> ok then 
//            //printfn "\r\nREQUEST =>\r\n%A\r\n" request
//            failwith "Authentication::Token Failed to renew. "
//        //printfn "\r\nRESPONSE =>\r\n%A\r\n" response
//        else 
//            let content = response |> getBody
//            let myToken = MsaToken.Parse content
//            let myReturn = myToken.AccessToken
//            //printfn "\r\nTOKEN =>\r\n%A\r\n" myReturn
//            myReturn
//    
//    //let getCurrentClientIDandSecret =
//    //    (CurrentClientIDandSecret.ClientId, CurrentClientIDandSecret.ClientSecret)
//    let setCurrentClientIDandSecret serviceId clientId clientSecret = 
//        if serviceId = StructuredContentServiceId then 
//            CurrentSCClientIDandSecret.ClientId <- clientId
//            CurrentSCClientIDandSecret.ClientSecret <- clientSecret
//        else 
//            CurrentFBClientIDandSecret.ClientId <- clientId
//            CurrentFBClientIDandSecret.ClientSecret <- clientSecret
//        ()
//    
//    let setFBClientIDandSecret clientId clientSecret = 
//        CurrentFBClientIDandSecret.ClientId <- clientId
//        CurrentFBClientIDandSecret.ClientSecret <- clientSecret
//        ()
//    
//    let setCurrentInstanceParams serviceID hostGuid = 
//        CurrentInstanceParams.HostGuid <- hostGuid
//        CurrentInstanceParams.ServiceId <- serviceID
//        ()
//    
//    let getTokenForSameClient (serviceID : string) (currentClientData : Dictionary<string, string>) = 
//        //let instanceParams = defaultActivationParameters
//        //let clientId, clientSecret = getCurrentClientIDandSecret
//        //currentClientData.Item("scope") <- permissions
//        let mutable currentClientId = ""
//        let mutable currentClientSecret = ""
//        if serviceID = StructuredContentServiceId then 
//            currentClientId <- CurrentSCClientIDandSecret.ClientId
//            currentClientSecret <- CurrentSCClientIDandSecret.ClientSecret
//        else 
//            currentClientId <- CurrentSCClientIDandSecret.ClientId
//            currentClientSecret <- CurrentSCClientIDandSecret.ClientSecret
//        let request = 
//            postTo (TokenServiceUrl())
//            |> Request.basicAuthentication currentClientId currentClientSecret
//            |> withFormBody currentClientData
//        
//        let response = request |> getResponse
//        if response.statusCode <> ok then 
//            //printfn "\r\nREQUEST =>\r\n%A\r\n" request
//            failwith "Authentication::Token Failed to retrieve. "
//            (null, currentClientId, currentClientSecret, currentClientData)
//        //printfn "\r\nRESPONSE =>\r\n%A\r\n" response
//        else 
//            let content = response |> getBody
//            let myToken = MsaToken.Parse content
//            let myReturn = myToken.AccessToken
//            (//printfn "\r\nTOKEN =>\r\n%A\r\n" myReturn
//             myReturn, currentClientId, currentClientSecret, currentClientData)
//    
//    //Public Function to getToken
//    //Parameter to provide:
//    // - ServiceID: To Identify which Micro Service. For instance: Form Builder or Structured Content
//    // - Permissions: A String combination of Permission Scope. For instance: "sc-type:read sc-type:write"
//    // - expectedClientId: This is a pre-defined clientId based on Test license account. 
//    let getToken (hostId:Guid, serviceId:string, permissions:string, expectedClientId:string) = 
//        let myToken = MsaToken.GetSample
//        let myReturn = ""
//        let startTime = System.DateTime.Now.TimeOfDay
//        
//        let myHostId = hostId
//        let instanceParams = 
//            { defaultActivationParameters with ServiceId = serviceId
//                                               HostGuid = myHostId }
//        //addLogEntry ("Auth", "ServiceId", instanceParams.ServiceId.ToString(), (System.DateTime.Now.TimeOfDay - startTime).ToString())
//        addLogEntry ("Auth", "HostGuid::ServiceId", instanceParams.HostGuid.ToString() + "::" + instanceParams.ServiceId.ToString(), "0")
//        let clientId, clientSecret = instanceParams |> getClientIdAndSecret (LicensingServiceUrl())
//        addLogEntry ("Auth", "clientId::clientSecret", clientId + "::" + clientSecret, (System.DateTime.Now.TimeOfDay - startTime).ToString())
//        if isNull clientId then (null, null, null, null)
//        else 
//            //test <@ clientId = expectedClientId @>
//            Assert.NotNull clientSecret
//            setCurrentClientIDandSecret serviceId clientId clientSecret
//            setCurrentInstanceParams instanceParams.ServiceId instanceParams.HostGuid
//            let clientData = Dictionary<string, string>()
//            [ "grant_type", "dnncustom"
//              "scope", permissions
//              "prodname", instanceParams.ProductName
//              "prodver", instanceParams.ProductVersion
//              "hostid", instanceParams.HostGuid.ToString()
//              "portalid", "0"
//              "userid", "1"
//              "username", "host"
//              "environment", "Production"
//              "clienttenantid", instanceParams.HostGuid.ToString() ]
//            |> Seq.iter clientData.Add
//            let request = 
//                postTo (TokenServiceUrl())
//                |> Request.basicAuthentication clientId clientSecret
//                |> withFormBody clientData
//            
//            let response = request |> getResponse
//            addLogEntry ("Auth", "Auth Done", response.contentLength.ToString(), (System.DateTime.Now.TimeOfDay - startTime).ToString())
//            if response.statusCode <> ok then 
//                //printfn "\r\nREQUEST =>\r\n%A\r\n" request
//                failwith "Authentication::Token Failed to retrieve. "
//                (null, null, null, null)
//            //printfn "\r\nRESPONSE =>\r\n%A\r\n" response
//            else 
//                let content = response |> getBody
//                let myToken = MsaToken.Parse content
//                let myReturn = myToken.AccessToken
//                addLogEntry ("Auth", "Token Reference", myReturn.ToString(), "0")
//                //printfn "\r\nTOKEN =>\r\n%A\r\n" myReturn
//                (myReturn, clientId, clientSecret, clientData)
//
//
