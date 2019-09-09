module Service.Config

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Prometheus
open Serilog


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
        |> Events.LogEventLevel.TryParse
        |> fun (result, value) -> if result then value else failwith "Invalid debug level"

    Log.Logger <- LoggerConfiguration()
      .Enrich.FromLogContext()
      .WriteTo.Console(formatter = Formatting.Compact.RenderedCompactJsonFormatter())
      .MinimumLevel.Is(level)
      .CreateLogger();

    builder.AddSerilog() |> ignore
