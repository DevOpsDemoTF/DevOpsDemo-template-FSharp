module Service.Metrics

open Prometheus

let metricServer = new KestrelMetricServer(port = 9102)
let healthCounter = Metrics.CreateCounter("health_counter", "Number of times the health endpoint has been called")
let unhandledErrorCounter = Metrics.CreateCounter("unhandled_error_counter", "Number of unhandled errors")


let initMetrics =
    fun () ->
        metricServer.Start() |> ignore
