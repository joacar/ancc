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
                // Without a custom validator only client certificates signed/issued by (?)
                // are allowed. Interested in how this is done.
                var validator = options.ApplicationServices.GetService<IClientCertificateValidator>();
                if (validator != null)
                {
                    connectionOptions.ClientCertificateValidation = validator.Validate;
                }
                
                configureHttpsDefaults?.Invoke(connectionOptions);
            });
        }
    }
}
