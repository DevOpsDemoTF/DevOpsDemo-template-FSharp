module Service.App

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Giraffe
open Serilog
open Microsoft.Extensions.Logging

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, sprintf "Unhandled exception: %s" ex.Message)
    Metrics.unhandledErrorCounter.Inc()
    clearResponse >=> setStatusCode 500 >=> text "An unhandled exception has occurred while executing the request."


[<EntryPoint>]
let main _ =
    Metrics.initMetrics()
    
    WebHostBuilder()
        .UseKestrel()
        .UseSerilog()
        .Configure(Action<IApplicationBuilder> (Config.configureApp Routing.webApp errorHandler))
        .ConfigureServices(Config.configureServices)
        .ConfigureLogging(Config.configureLogging)
        .UseUrls("http://0.0.0.0:8080")
        .Build()
        .Run()
    0
