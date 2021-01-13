using System;
using System.Net.Http;
using System.Net.Security;
using System.Reflection;

using AppleSystemStatus.Services;

using AutoMapper;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(AppleSystemStatus.Startup))]

namespace AppleSystemStatus
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddDbContext<AppleSystemStatusDbContext>((provider, context) =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var databaseConnectionString = configuration.GetValue<string?>("DatabaseConnectionString");
                if (databaseConnectionString is null)
                {
                    var vaultUri = configuration.GetValue<Uri>("VaultUri");
                    var secretName = configuration.GetValue<string>("VaultSecret");
                    var credential = new ManagedIdentityCredential();
                    var secretClient = new SecretClient(vaultUri, credential);
                    databaseConnectionString = secretClient.GetSecret(secretName)?.Value?.Value;
                }
                context.UseSqlServer(databaseConnectionString);
            });

            builder.Services
                .AddHttpClient<SystemStatusService>(client => client.BaseAddress = new Uri("https://www.apple.com"))
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => sslPolicyErrors == SslPolicyErrors.None || sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch && certificate.SubjectName.Name.StartsWith("CN=www.apple.com") });

            builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

            builder.Services.AddScoped<RepositoryService>();

            builder.Services.AddHealthChecks().AddDbContextCheck<AppleSystemStatusDbContext>();
        }
    }
}