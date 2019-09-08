module Service.Config

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Prometheus


let configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore

let configureApp webApp errorHandler (app : IApplicationBuilder) =
    app.UseGiraffeErrorHandler(errorHandler)
        .UseHttpMetrics()
        .UseGiraffe(webApp)
    
let configureLogging (builder : ILoggingBuilder) =
    let filter (l : LogLevel) = l.Equals LogLevel.Error
    builder.AddFilter(filter).AddConsole() |> ignore
