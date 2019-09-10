FROM mcr.microsoft.com/dotnet/core/sdk:2.2-alpine AS build
WORKDIR /app

# bootstrap paket dependency manager
RUN dotnet tool install paket --tool-path /usr/local/bin

# copy csproj and restore as distinct layers
COPY . .
RUN paket restore

# copy and publish app and libraries
RUN dotnet publish -c Release -o out

# test application
RUN dotnet test --logger:"junit;LogFilePath=/app/test-results.xml"


FROM mcr.microsoft.com/dotnet/core/runtime:2.2-alpine

RUN addgroup -S app && adduser -S app -G app

EXPOSE 8080
EXPOSE 9102

ENV DEBUG_LEVEL "DEBUG"

COPY --from=build /app/src/App/out/* /app/test-results.xml /app/

USER app
WORKDIR /app
ENTRYPOINT ["dotnet", "App.dll"]
