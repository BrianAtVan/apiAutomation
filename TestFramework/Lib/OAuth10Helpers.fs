namespace TestFramework.Lib
open System

module OAuth10Helpers =

    type ParameterKeyValue = KeyValue of string * string

    type HashAlgorithm = HMACSHA1 | PLAINTEXT | RSASHA1

    type HttpMethod = GET | POST

    type ConsumerInfo = { consumerKey : string; consumerSecret : string; consumerCallback : string }
    type RequestInfo = { requestToken : string; requestSecret : string }
    type AccessInfo = { accessToken : string; accessSecret : string }

    type UseFor = ForRequestToken of ConsumerInfo
                | ForAccessToken of ConsumerInfo * RequestInfo * string
                | ForWebService of ConsumerInfo * AccessInfo * ParameterKeyValue list

    type HttpRequirement = Requirement of System.Text.Encoding * string * HttpMethod

    [<CompiledName("ConcatStringsWithToken")>]
    let inline concatStringsWithToken token s1 s2 =
        if s1 = "" then s2 else s1 + token + s2

    [<CompiledName("ConcatSecretKeys")>]
    let concatSecretKeys = function
        | x::y::xs -> List.fold (concatStringsWithToken "&") "" (x::y::xs)
        | x::xs -> x + "&"
        | _ -> ""

    [<CompiledName("UrlEncode")>]
    let urlEncode (encode : System.Text.Encoding) (urlString : string) =
        let urlBytes = encode.GetBytes urlString |> List.ofArray
        let encodeChar b =
            let validChars = List.ofSeq "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~"
            if b < 128uy && List.exists (fun v -> v = char b) validChars then (char b).ToString()
            else
                System.String.Format ("%{0:X2}", b)
        urlBytes
        |> List.map encodeChar
        |> List.fold (fun s1 s2 -> s1 + s2) ""

    [<CompiledName("Require")>]
    let require encoding targetUrl httpMethod = Requirement (encoding, targetUrl, httpMethod)

    [<CompiledName("GetHttpMethodString")>]
    let getHttpMethodString = function
        | GET -> "GET"
        | POST -> "POST"

    [<CompiledName("ToKeyValue")>]
    let toKeyValue tupleList = List.map KeyValue tupleList

    [<CompiledName("FromKeyValue")>]
    let fromKeyValue keyValues = List.map (fun (KeyValue (key, value)) -> (key, value)) keyValues

    [<CompiledName("HeaderParameter")>]
    let headerParameter keyValues =
        match keyValues with
        | [] -> ""
        | _ ->
            keyValues
            |> List.map (fun (KeyValue (key, value)) ->
                            key + "=\"" + value + "\"")
            |> List.fold (concatStringsWithToken ", ") ""

    [<CompiledName("Parameterize")>]
    let parameterize encoder keyValue =
        let (KeyValue (key, value)) = keyValue
        key + "=" + (encoder value)

    [<CompiledName("ToParameter")>]
    let toParameter encoder keyValues =
        let parameterized = keyValues |> List.map (parameterize encoder)
        match parameterized with
        | x::y::xs ->  List.fold (concatStringsWithToken "&") "" parameterized
        | x::xs -> x + "&"
        | _ -> ""

    [<CompiledName("FromParameter")>]
    let fromParameter (parameterString : string) =
        parameterString.Split [|'&'|]
        |> List.ofArray
        |> List.map ((fun (s : string) -> s.Split [|'='|] ) >>
                    (fun kv -> KeyValue (kv.[0], kv.[1])))

    [<CompiledName("TryGetValue")>]
    let tryGetValue key keyValues =
        List.tryPick (fun (KeyValue (k, v)) ->
                    if k = key then Some v else None) keyValues

    type System.Net.WebClient with

    [<CompiledName("AsyncUploadString")>]
    member this.AsyncUploadString (address:Uri) meth data : Async<string> =
        let uploadAsync =
            Async.FromContinuations (fun (cont, econt, ccont) ->
                let userToken = new obj()
                let rec handler =
                    System.Net.UploadStringCompletedEventHandler (fun _ args ->
                        if userToken = args.UserState then
                            this.UploadStringCompleted.RemoveHandler(handler)
                        if args.Cancelled then
                            ccont (new OperationCanceledException())
                        elif args.Error <> null then
                            econt args.Error
                        else
                            cont args.Result
                    )
                this.UploadStringCompleted.AddHandler(handler)
                this.UploadStringAsync(address, meth, data, userToken)
            )
        async {
            use! _holder = Async.OnCancel(fun _ -> this.CancelAsync())
            return! uploadAsync
        }