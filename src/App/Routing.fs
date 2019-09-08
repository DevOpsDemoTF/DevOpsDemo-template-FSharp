module Service.Routing

open Giraffe
open Service.Handlers
open Microsoft.AspNetCore.Http


let webApp : HttpFunc -> HttpContext -> HttpFuncResult =
    choose [
        GET >=> choose [
            route "/health" >=> handleHealthCheck
        ]
        setStatusCode 404 >=> text "Not Found" ]
