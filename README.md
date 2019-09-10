# Template for micro-service in F# #
[![Build Status](https://dev.azure.com/butzist/DevOpsDemo/_apis/build/status/DevOpsDemoTF.DevOpsDemo-template-FSharp?branchName=master)](https://dev.azure.com/butzist/DevOpsDemo/_build/latest?definitionId=4&branchName=master)

### Description ###
Micro-service template to use with my [DevOpsDemo](https://github.com/butzist/DevOpsDemo)

### Features ###
* Build in multi-stage Docker container
* Configuration via environment variables
* Logging in JSON
* Health-check endpoint
* Prometheus metrics
* Unit tests with xunit-compatible output
* API/integration tests with docker-compose

### Links ###
* [Using Prometheus metrics](https://github.com/prometheus-net/prometheus-net#counters)
* [Web services with F#](https://devblogs.microsoft.com/dotnet/build-a-web-service-with-f-and-net-core-2-0/)
* [F# with .NET Core CLI](https://docs.microsoft.com/en-us/dotnet/fsharp/get-started/get-started-command-line)
* [Build .NET Core in Docker](https://github.com/dotnet/dotnet-docker/blob/master/samples/dotnetapp/README.md)