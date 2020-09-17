using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Hosting;

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
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .ConfigureKestrel(options =>
                        {
                            options.ConfigureHttpsDefaults(https =>
                            {
                                https.ClientCertificateValidation += (certificate2, chain, errors) =>
                                {
                                    // TODO: How to ensure that client certificates issued by server certificate are allowed?
                                    return true;
                                };
                                https.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                                https.ServerCertificate = CreateServerCertificate();
                            });
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
