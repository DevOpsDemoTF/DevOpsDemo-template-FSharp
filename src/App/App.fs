module Service.App

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
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
    let config = Config.getConfiguration
    
    WebHostBuilder()
        .UseKestrel()
        .UseSerilog()
        .Configure(Action<IApplicationBuilder> (Config.configureApp Routing.webApp errorHandler))
        .ConfigureServices( fun services ->
            services
                .AddSingleton(State.create config)
                .AddGiraffe()
                |> ignore
        )
        .ConfigureLogging(Config.configureLogging config)
        .UseUrls("http://0.0.0.0:8080")
        .Build()
        .Run()
    0
