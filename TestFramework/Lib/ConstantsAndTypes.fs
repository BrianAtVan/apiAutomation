namespace TestFramework.Lib

open System.Configuration
open FSharp.Data

[<AutoOpen>]
module ConstantsAndTypes =

    type TestEnvironmentType = | QA | DEV | PROD | STAGING
    type ApiReturnDataType = | Json | Rest | Yaml      

    let mutable TestEnvrionment : TestEnvironmentType = QA //Default is QA

    /// setup the environment from the onfig file
    let setTestEnvironmentFromConfig() =
        let env = ConfigurationManager.AppSettings.["TargetApiEnvironment"]
        let env2 = if isNull env then "" else env.ToUpperInvariant()
        match env2 with
        | "DEV"  -> TestEnvrionment <- DEV
        | "PROD" -> TestEnvrionment <- PROD
        | "STAGING" -> TestEnvrionment <- STAGING
        | _      -> TestEnvrionment <- QA


    // for local testing replace the tested environment with "http://localhost:21141"
    let [<Literal>] EventfulAppKey = "Tz3cPsqC8n6w7ghC"
    let [<Literal>] private EventfulUrlDev = "http://api.eventful.com/json"
    let [<Literal>] private EventfulUrlQA = "http://api.eventful.com/json"
    let [<Literal>] private EventfulUrlProd = "https://api.eventful.com/json"
    let [<Literal>] private EventfulUrlStaging = "https://api.eventful.com/json" 
    let EventfulUrl() =
        match TestEnvrionment with
        | DEV ->  EventfulUrlDev
        | PROD -> EventfulUrlProd
        | STAGING -> EventfulUrlStaging
        | _ ->    EventfulUrlQA

    let [<Literal>] private EventfulRequestOAuthTokenUrlProd = "http://eventful.com/oauth/request_token"
    let [<Literal>] private EventfulRequestOAuthTokenUrlQA = "http://eventful.com/oauth/request_token"
    let EventfulRequestTokenUrl() =
        match TestEnvrionment with
        | DEV ->  EventfulRequestOAuthTokenUrlQA
        | PROD -> EventfulRequestOAuthTokenUrlProd
        | STAGING -> EventfulRequestOAuthTokenUrlProd
        | _ ->    EventfulRequestOAuthTokenUrlQA

    let [<Literal>] DefaultUserAgent = 
        "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.87 Safari/537.36"

    type MsaToken = JsonProvider< """ {"access_token": "string", "expires_in": 3600, "token_type": "Bearer"} """ >

    
 
    
    let [<Literal>] private SampleSearchResult = """{"last_item":null,"total_items":"682","first_item":null,"page_number":"1","page_size":"10","page_items":null,"search_time":"0.048","page_count":"69","events":{"event":[{"watching_count":null,"olson_path":"America/Los_Angeles","calendar_count":null,"comment_count":null,"region_abbr":"CA","postal_code":"92111","going_count":null,"all_day":"0","latitude":"32.8317369","groups":null,"url":"http://sandiego.eventful.com/events/san-diego-book-discussion-group-/E0-001-106765432-5?utm_source=apis&utm_medium=apim&utm_campaign=apic","id":"E0-001-106765432-5","privacy":"1","city_name":"San Diego","link_count":null,"longitude":"-117.1651930","country_name":"United States","country_abbr":"USA","region_name":"California","start_time":"2017-10-18 19:00:00","tz_id":null,"description":" <p><p>Ramses the Great has awakened in Edwardian London. Having drunk the elixir of life, he is now Ramses the Damned, doomed forever to wander the earth, desperate to quell hungers that can never be satisfied. Although he pursues voluptuous aristocrat Julie Stratford, the woman for whom he desperately longs is Cleopatra. And his intense longing for her, undiminished over the centuries, will force him to commit an act that will place everyone around him in the gravest danger....</p></p>","modified":"2017-09-16 04:58:59","venue_display":"1","tz_country":null,"performers":null,"title":"San Diego Book Discussion Group","venue_address":"5943 Balboa Avenue, Suite #100","geocode_type":"EVDB Geocoder","tz_olson_path":null,"recur_string":null,"calendars":null,"owner":"evdb","going":null,"country_abbr2":"US","image":null,"created":"2017-09-16 04:58:59","venue_id":"V0-001-000104270-1","tz_city":null,"stop_time":null,"venue_name":"Mysterious Galaxy Books","venue_url":"http://sandiego.eventful.com/venues/mysterious-galaxy-books-/V0-001-000104270-1?utm_source=apis&utm_medium=apim&utm_campaign=apic"}]}}"""
    type TypeEventfulSearchResultJson = JsonProvider<SampleSearchResult>
    
    let [<Literal>] private SampleVenuesSearchResult = """{"last_item":null,"version":"0.2","total_items":"228","first_item":null,"page_number":"1","page_size":"10","page_items":null,"search_time":"0.058","page_count":"23","venues":{"venue":[{"geocode_type":"Zip Code Based GeoCodes","event_count":"0","trackback_count":"0","comment_count":"0","region_abbr":"CA","postal_code":"92026","latitude":"33.2223","url":"http://sandiego.eventful.com/venues/restaurant-/V0-001-007843183-7?utm_source=apis&utm_medium=apim&utm_campaign=apic","id":"V0-001-007843183-7","address":"534 grane","city_name":"Escondido","owner":"evdb","link_count":"0","country_name":"United States","longitude":"-117.112","timezone":null,"country_abbr":"USA","region_name":"California","country_abbr2":"US","name":"restaurant","description":null,"image":null,"created":null,"venue_type":"address","venue_name":"restaurant"}]}}"""
    type TypeEventfulVenuesSearchResultJson = JsonProvider<SampleVenuesSearchResult>

    let [<Literal>] private SampleVenuesGetResult = """{"withdrawn":null,"children":null,"comments":null,"region_abbr":"CA","postal_code":"92026","latitude":"33.2223","url":"//sandiego.eventful.com/venues/restaurant-/V0-001-007843183-7?utm_source=apis&utm_medium=apim&utm_campaign=apic","id":"V0-001-007843183-7","address":"534 grane","metro":"San Diego metro area","links":null,"images":null,"withdrawn_note":null,"longitude":"-117.112","country_abbr":"USA","name":"restaurant","region":"California","description":null,"properties":null,"modified":"2014-03-29 02:37:01","venue_display":"1","parents":null,"geocode_type":"Zip Code Based GeoCodes","tz_olson_path":"America/Los_Angeles","city":"Escondido","trackbacks":null,"country":"United States","owner":"evdb","country_abbr2":"US","tags":null,"venue_type":"address","created":"2014-03-28 23:39:25","events":null}"""
    type TypeEventfulVenuesGetResultJson = JsonProvider<SampleVenuesGetResult>
    
    let [<Literal>] private SampleErrorResult = """{"status":"Missing parameter","error":"1","description":"The 'id' parameter is mandatory."}"""
    type TypeEventfulErrorResultJson = JsonProvider<SampleErrorResult>