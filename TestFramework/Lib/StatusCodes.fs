namespace TestFramework.Lib

open System.Net

[<AutoOpen>]
module StatusCodes =

    let ok = int HttpStatusCode.OK // 200
    let created = int HttpStatusCode.Created // 201
    let accepted = int HttpStatusCode.Accepted // 202
    let badRequest = int HttpStatusCode.BadRequest // 400
    let unauthorized = int HttpStatusCode.Unauthorized // 401
    let forbidden = int HttpStatusCode.Forbidden // 403
    let notFound = int HttpStatusCode.NotFound // 404
    let notAcceptable = int HttpStatusCode.NotAcceptable // 406
    let requestTimeout = int HttpStatusCode.RequestTimeout // 408
    let conflict = int HttpStatusCode.Conflict // 409
    let gone = int HttpStatusCode.Gone // 410
    let unsupportedMediaType = int HttpStatusCode.UnsupportedMediaType // 415
    let internalServerError = int HttpStatusCode.InternalServerError // 500
    let notImplemented = int HttpStatusCode.NotImplemented // 501
    let badGateway = int HttpStatusCode.BadGateway // 502
    let serviceUnavailable = int HttpStatusCode.ServiceUnavailable // 503
