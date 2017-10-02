namespace TestFramework.Lib

//Adapted from: https://github.com/CarstenKoenig/FRest/blob/master/FRest.Client/AsyncRequests.fs

open System
open RestSharp

[<AutoOpen>]
module SyncRequests = 

    /// <summary>Sends a synchronous request and receives a response from a resource.</summary>
    /// <param name="url">URL for the protocol and domain of the resource.</param>
    /// <param name="pathAndQuery">THe relative path (and query params when any) for the resource to retrieve.</param>
    /// <param name="addData">POST or PUT data to add to the request body. This must be formatted as needed to go on the wire.</param>
    /// <returns><see cref="ErrorOrSuccess"/> that contains either an <see cref="IRestResponse"/> object 
    /// with the call result or an Exception if there was one.</returns>
    let requestWith (url : Url) (pathAndQuery : string, verb : Method) (addData : IRestRequest -> IRestRequest) : ErrorOrSuccess<IRestResponse> =
        let uri = Uri(url + pathAndQuery)
        let client  = RestClient(uri)
        let request = 
            RestRequest("", verb)
            |> addData

        let resp = client.Execute(request)

        printfn "%A %A" resp.ResponseStatus uri
        match resp.ResponseStatus with
        | ResponseStatus.Aborted ->
            raise (OperationCanceledException())
        | ResponseStatus.Completed ->
            printfn "Status: %A" resp.StatusCode
            resp
            |> ErrorOrSuccess.succeededWith 
            //|> result
        | ResponseStatus.Error ->
            printfn "Erro Message: %A" resp.ErrorException.Message
            match resp.ErrorException with
            | :? Failed as f ->
                ErrorOrSuccess.failedWith f
            | _ as err when not (isNull err) ->
                ErrorOrSuccess.failedWith err
            | _ ->
                failwith "unknown server exception" 
        | ResponseStatus.TimedOut ->
            ErrorOrSuccess.failedWith (TimeoutException())
        | ResponseStatus.None ->
            failwith "no reply" 
        | _ -> failwith "Unhandeled ResponseStatus"

    let private addXmlBody (body : 'b) (request : IRestRequest) : IRestRequest =
        request.AddXmlBody body

    let private addJsonBody (body : 'b) (request : IRestRequest) : IRestRequest =
        request.AddJsonBody body

    let private noData (request : IRestRequest) : IRestRequest =
        request

    let get (url : Url) (pathAndQuery : string) : ErrorOrSuccess<IRestResponse> =
        requestWith url (pathAndQuery, Method.GET) noData

    let delete (url : Url) (pathAndQuery : string) : ErrorOrSuccess<IRestResponse> =
        requestWith url (pathAndQuery, Method.DELETE) noData

    let postAsXml (url : Url) (pathAndQuery : string, data : 'b) : ErrorOrSuccess<IRestResponse> =
        requestWith url (pathAndQuery, Method.POST) (addXmlBody data)

    let postAsJson (url : Url) (pathAndQuery : string, data : 'b) : ErrorOrSuccess<IRestResponse> =
        requestWith url (pathAndQuery, Method.POST) (addJsonBody data)

    let putAsXml (url : Url) (pathAndQuery : string, data : 'b) : ErrorOrSuccess<IRestResponse> =
        requestWith url (pathAndQuery, Method.PUT) (addXmlBody data)

    let putAsJson (url : Url) (pathAndQuery : string, data : 'b) : ErrorOrSuccess<IRestResponse> =
        requestWith url (pathAndQuery, Method.PUT) (addJsonBody data)

    // The following functions use a wrapper around the Async methods

    let postXmlSync (url : Url) (pathAndQuery : string, data : 'b) : ErrorOrSuccess<'a> = 
        let callAsync = async { let! response = postXmlAsync url (pathAndQuery, data)
                                return response }
        Async.RunSynchronously callAsync
    
    let postJsonSync (url : Url) (pathAndQuery : string, data : 'b) : ErrorOrSuccess<'a> = 
        let callAsync = async { let! response = postJsonAsync url (pathAndQuery, data)
                                return response }
        Async.RunSynchronously callAsync
    
    let putXmlSync (url : Url) (pathAndQuery : string, data : 'b) : ErrorOrSuccess<'a> = 
        let callAsync = async { let! response = putXmlAsync url (pathAndQuery, data)
                                return response }
        Async.RunSynchronously callAsync
    
    let putJsonSync (url : Url) (pathAndQuery : string, data : 'b) : ErrorOrSuccess<'a> = 
        let callAsync = async { let! response = putJsonAsync url (pathAndQuery, data)
                                return response }
        Async.RunSynchronously callAsync
