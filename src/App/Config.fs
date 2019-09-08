module Service.Config

open System
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
    let level =
        System.Environment.GetEnvironmentVariable("DEBUG_LEVEL") |> Option.ofObj
        |> Option.defaultValue "DEBUG"
        |> String.mapi (fun i c -> (if i = 0 then System.Char.ToUpper else System.Char.ToLower) c)
        |> LogLevel.TryParse
        |> fun (result, value) -> if result then Some(value) else None
        |> Option.orElseWith (failwith "Invalid debug level")

    let filter (l : LogLevel) = l >= level.Value
    builder.AddFilter(filter).AddConsole() |> ignore
