FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
ARG VERSION=0.0.0
COPY . .
RUN dotnet publish -c Release -o /home/site/wwwroot /p:Version=${VERSION}
FROM mcr.microsoft.com/azure-functions/dotnet:3.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot
COPY --from=build ["/home/site/wwwroot", "/home/site/wwwroot"]