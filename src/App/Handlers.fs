module Service.Handlers

open Microsoft.AspNetCore.Http
open Giraffe
open FSharp.Control.Tasks.ContextInsensitive
open Metrics


let handleHealthCheck =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            healthCounter.Inc()
            return! text "" next ctx
        }
