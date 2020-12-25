FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
ARG DATABASE_CONNECTION_STRING
COPY . /usr/local/src
WORKDIR /usr/local/src/AppleSystemStatus/AppleSystemStatus
RUN dotnet ef database update --connection ${DATABASE_CONNECTION_STRING} && \
    dotnet publish -c Release -o /home/site/wwwroot
FROM mcr.microsoft.com/azure-functions/dotnet:3.0
ARG AZURE_WEBJOBS_STORAGE
ARG DATABASE_CONNECTION_STRING
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    AzureWebJobsStorage=${AZURE_WEBJOBS_STORAGE} \
    DatabaseConnectionString=${DATABASE_CONNECTION_STRING} \
    AzureWebJobs.SystemStatusHttp.Disabled=true \
    AzureWebJobs.StoresImportHttp.Disabled=true
COPY --from=build ["/home/site/wwwroot", "/home/site/wwwroot"]