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
                    services.AddSingleton<IClientCertificateValidator, MutualTls>();
                    services.Configure<ClientCertificateValidationOptions>(options =>
                    {
                        options.Issuer = "CN=ancc_CARoot";
                        options.Thumbprint = "df40194d88ba62ef246f5643c5ad5719bd3c6452";
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
