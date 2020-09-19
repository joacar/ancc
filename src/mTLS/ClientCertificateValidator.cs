using Microsoft.Extensions.Options;
using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace ancc.mTLS
{
    internal sealed class ClientCertificateValidator : IClientCertificateValidator
    {
        private readonly ClientCertificateValidationOptions _options;

        public ClientCertificateValidator(IOptions<ClientCertificateValidationOptions> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options.Value));
            if (string.IsNullOrEmpty(_options.Issuer))
            {
                throw new ArgumentException("Issuer must be set", nameof(ClientCertificateValidationOptions.Issuer));
            }
        }

        public bool Validate(X509Certificate2 clientCertificate, X509Chain chain, SslPolicyErrors errors)
        {
            // TODO: How can I check that the client certificate is signed by
            //  1. Root CA
            //  2. Server certificate
            // which ever is most correct

            if (!clientCertificate.Verify())
            {
                return false;
            }

            if (!string.Equals(_options.Issuer, clientCertificate.Issuer, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (string.Equals(_options.Thumbprint, clientCertificate.Thumbprint, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}