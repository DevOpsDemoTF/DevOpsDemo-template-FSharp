module Service.Handlers

open Microsoft.AspNetCore.Http
open Giraffe
open FSharp.Control.Tasks.ContextInsensitive


let handleHealthCheck =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            Metrics.healthCounter.Inc()
            return! text "" next ctx
        }
