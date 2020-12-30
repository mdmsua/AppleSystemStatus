FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
ARG DATABASE_CONNECTION_STRING
ENV DatabaseConnectionString=${DATABASE_CONNECTION_STRING}
COPY . .
RUN dotnet tool restore && \
    dotnet ef database update --project AppleSystemStatus --connection "${DATABASE_CONNECTION_STRING}" && \
    dotnet publish -c Release -o /home/site/wwwroot
FROM mcr.microsoft.com/azure-functions/dotnet:3.0
ARG AZURE_WEBJOBS_STORAGE
ARG DATABASE_CONNECTION_STRING
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    AzureWebJobsStorage=${AZURE_WEBJOBS_STORAGE} \
    DatabaseConnectionString=${DATABASE_CONNECTION_STRING}
COPY --from=build ["/home/site/wwwroot", "/home/site/wwwroot"]