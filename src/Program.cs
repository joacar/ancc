using ancc.mTLS;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Security.Cryptography.X509Certificates;

namespace ancc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    // For custom validation register instance of IClientCertificateValidator
                    //services.AddSingleton<IClientCertificateValidator, ClientCertificateValidator>();
                    services.Configure<ClientCertificateValidationOptions>(options =>
                    {
                        options.Issuer = "CN=localhost";
                        options.Thumbprint = "160e94deb46511a0a2ee07fef020b9a9d9da422c";
                        options.Certificate = CreateServerCertificate();
                    });
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .ConfigureKestrel(options =>
                        {
                            options.ConfigureMutualTLS();
                        });
                });

        private static X509Certificate2 CreateServerCertificate()
        {
            // This is how Kestrel sets DefaultCertificate but I having trouble retrieving it
            // to use in validation.
            const string envFilePath = "ASPNETCORE_Kestrel__Certificates__Default__Path";
            var cert = Environment.GetEnvironmentVariable(envFilePath);
            if (string.IsNullOrEmpty(cert))
            {
                throw new InvalidOperationException($"Path to certificate (pfx) MUST be specified in environment variable {envFilePath}");
            }

            const string envPassword = "ASPNETCORE_Kestrel__Certificates__Default__Password";
            var password = Environment.GetEnvironmentVariable(envPassword);
            if (string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException($"Certificate password MUST be specified in environment variable {envPassword}");
            }

            return new X509Certificate2(cert, password);
        }
    }

}
