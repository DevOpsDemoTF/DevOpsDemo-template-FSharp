module Service.Config

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Logging
open Giraffe
open Prometheus
open Serilog

type Configuration =
    {
        LogLevel: string
    }

let getConfiguration =
    {
        LogLevel =
            System.Environment.GetEnvironmentVariable("DEBUG_LEVEL") |> Option.ofObj
            |> Option.defaultValue "WARNING"
 
    }

let configureApp webApp errorHandler (app : IApplicationBuilder) =
    app.UseGiraffeErrorHandler(errorHandler)
        .UseHttpMetrics()
        .UseGiraffe(webApp)
    
let configureLogging configuration (builder : ILoggingBuilder) =
    let level =
        configuration.LogLevel
        |> String.mapi (fun i c -> (if i = 0 then System.Char.ToUpper else System.Char.ToLower) c)
        |> Events.LogEventLevel.TryParse
        |> fun (result, value) -> if result then value else failwith "Invalid debug level"

    Logging.initLogger level
    builder.AddSerilog() |> ignore
