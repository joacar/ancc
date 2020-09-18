using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ancc.mTLS
{
    public static class KestrelHttpsExtensions
    {
        public static void ConfigureMutualTLS(
            this KestrelServerOptions options, 
            Action<HttpsConnectionAdapterOptions> configureHttpsDefaults = null)
        {
            options.ConfigureHttpsDefaults(connectionOptions =>
            {
                connectionOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                // The ServerCertificate will be set if ASPNETCORE_Kestrel__Certificates__Default__{Path,Password} is set correctly.
                // It is set after this method has finished so it's not directly available here,
                // but is when validation callback is called. Inspection the IOptions<HttpsCon..>
                // shows that it's null.
                var validator = options.ApplicationServices.GetRequiredService<IClientCertificateValidator>();
                connectionOptions.ClientCertificateValidation = validator.Validate;

                configureHttpsDefaults?.Invoke(connectionOptions);
            });
        }
    }
}
