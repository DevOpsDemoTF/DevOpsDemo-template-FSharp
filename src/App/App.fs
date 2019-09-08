module Service.App

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Giraffe
open Service.Config
open Service.Routing

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    //TODO increase prometheus error counter
    clearResponse >=> setStatusCode 500 >=> text ex.Message


[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        .Configure(Action<IApplicationBuilder> (configureApp webApp errorHandler))
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .UseUrls("http://0.0.0.0:8080")
        .Build()
        .Run()
    0
