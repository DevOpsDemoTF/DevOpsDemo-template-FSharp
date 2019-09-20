module Service.Handlers

open Microsoft.AspNetCore.Http
open Giraffe
open FSharp.Control.Tasks.ContextInsensitive
open Microsoft.Extensions.DependencyInjection



let handleHealthCheck =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            Metrics.healthCounter.Inc()
            let state = ctx.GetService<State.State>()
            ctx.SetStatusCode (if state.healthy then 200 else 500)
            return! text "" next ctx
        }
