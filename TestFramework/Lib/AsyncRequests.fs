namespace TestFramework.Lib

//Adapted from: https://github.com/CarstenKoenig/FRest/blob/master/FRest.Client/AsyncRequests.fs

open System
open RestSharp

type Url = string
exception Failed of int * string

[<AutoOpen>]
module AsyncRequests =

    /// <summary>Sends an asynchronous request and receives a response from a resource.</summary>
    /// <param name="url">URL for the protocol and domain of the resource.</param>
    /// <param name="pathAndQuery">THe relative path (and query params when any) for the resource to retrieve.</param>
    /// <param name="addData">POST or PUT data to add to the request body. This must be formatted as needed to go on the wire.</param>
    /// <returns><see cref="ErrorOrSuccess"/> that contains either an <see cref="Object"/> object 
    /// with the call result as (XML, JSON, etc.) an Exception if there was one.</returns>
    let requestWithAsync (url : Url) (pathAndQuery : string, verb : Method) (addData : IRestRequest -> IRestRequest) : Async<ErrorOrSuccess<'a>> =
        let uri = Uri(url + pathAndQuery)
        let client  = RestClient(uri)
        let request = 
            RestRequest("", verb)
            |> addData

        Async.FromContinuations(fun (result, error, cancel) ->
            client.ExecuteAsync(
                request, 
                Action<IRestResponse<'a>> (fun resp -> 
                    printfn "%A %A" resp.ResponseStatus uri
                    match resp.ResponseStatus with
                    | ResponseStatus.Aborted ->
                        OperationCanceledException () 
                        |> cancel
                    | ResponseStatus.Completed ->
                        printfn "Status: %A" resp.StatusCode
                        resp.Data
                        |> ErrorOrSuccess.succeededWith 
                        |> result
                        (*
                        match resp.statusCode with
                        | Net.HttpStatusCode.OK // 200
                        | Net.HttpStatusCode.Created // 201
                        | Net.HttpStatusCode.Accepted -> // 202
                            resp.Data 
                            |> ErrorOrSuccess.succeededWith 
                            |> result
                        | _ -> exn (resp.StatusDescription) |> error
                        *)
                    | ResponseStatus.Error ->
                        printfn "Erro Message: %A" resp.ErrorException.Message
                        match resp.ErrorException with
                        | :? Failed as f ->
                            ErrorOrSuccess.failedWith f
                            |> result
                        | :? exn as err when not (isNull err) ->
                            error err
                        | _ ->
                            exn "unknown server exception" 
                            |> error
                    | ResponseStatus.TimedOut ->
                        TimeoutException () 
                        |> error
                    | ResponseStatus.None ->
                        exn "no reply" 
                        |> error
                    | _ -> failwith "Unhandeled ResponseStatus"))
            |> ignore)

    let private addXmlBody (body : 'b) (request : IRestRequest) : IRestRequest =
        request.AddXmlBody body

    let private addJsonBody (body : 'b) (request : IRestRequest) : IRestRequest =
        request.AddJsonBody body

    let private noData (request : IRestRequest) : IRestRequest =
        request

    let getAsync (url : Url) (pathAndQuery : string) : Async<ErrorOrSuccess<'a>> =
        requestWithAsync url (pathAndQuery, Method.GET) noData

    let deleteAsync (url : Url) (pathAndQuery : string) : Async<ErrorOrSuccess<'a>> =
        requestWithAsync url (pathAndQuery, Method.DELETE) noData

    let postXmlAsync (url : Url) (pathAndQuery : string, data : 'b) : Async<ErrorOrSuccess<'a>> =
        requestWithAsync url (pathAndQuery, Method.POST) (addXmlBody data)

    let postJsonAsync (url : Url) (pathAndQuery : string, data : 'b) : Async<ErrorOrSuccess<'a>> =
        requestWithAsync url (pathAndQuery, Method.POST) (addJsonBody data)

    let putXmlAsync (url : Url) (pathAndQuery : string, data : 'b) : Async<ErrorOrSuccess<'a>> =
        requestWithAsync url (pathAndQuery, Method.PUT) (addXmlBody data)

    let putJsonAsync (url : Url) (pathAndQuery : string, data : 'b) : Async<ErrorOrSuccess<'a>> =
        requestWithAsync url (pathAndQuery, Method.PUT) (addJsonBody data)
