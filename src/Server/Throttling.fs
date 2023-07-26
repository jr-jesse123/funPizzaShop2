

module FunPizza.Server.Throttling

open Microsoft.AspNetCore.Http
open ThrottlingTroll

let setOptions (options: ThrottlingTrollOptions) =

    let idByIp = fun (request:IHttpRequestProxy) -> 
        let r = (request :?> IIncomingHttpRequestProxy).Request
        r.HttpContext.Connection.RemoteIpAddress.ToString()

    let PerMinute c = FixedWindowRateLimitMethod(PermitLimit = c, IntervalInSeconds = 60)
    

    let config = ThrottlingTrollConfig()
    let login =  
        ThrottlingTrollRule(
            UriPattern="/api/Authentication/Login",
            LimitMethod = PerMinute 5,
            IdentityIdExtractor = idByIp)
    let verify = ThrottlingTrollRule(
            UriPattern="/api/Authentication/Verify",
            LimitMethod = PerMinute 7,
            IdentityIdExtractor = idByIp)
    let verify2 = ThrottlingTrollRule(
            UriPattern="/api/Authentication/Verify",
            LimitMethod = FixedWindowRateLimitMethod(PermitLimit = 15, IntervalInSeconds = 600),
            IdentityIdExtractor = idByIp)

    config.Rules <- [|login; verify; verify2|]