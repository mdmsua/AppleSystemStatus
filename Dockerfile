FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
ARG DATABASE_CONNECTION_STRING
ARG VERSION=0.0.0
ENV DatabaseConnectionString=${DATABASE_CONNECTION_STRING}
COPY . .
RUN dotnet tool restore && \
    dotnet ef database update --project AppleSystemStatus && \
    dotnet publish -c Release -o /home/site/wwwroot /p:Version=${VERSION}
FROM mcr.microsoft.com/azure-functions/dotnet:3.0
ARG VAULT_URI
ARG VAULT_SECRET
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    AzureWebJobsStorage=${AZURE_WEBJOBS_STORAGE} \
    VaultUri=${VAULT_URI} \
    VaultSecret=${VAULT_SECRET}
COPY --from=build ["/home/site/wwwroot", "/home/site/wwwroot"]