using System.Security.Cryptography.X509Certificates;

namespace ancc.mTLS
{
    public sealed class ClientCertificateValidationOptions
    {
        /// <summary>
        /// Gets or sets the issuer that client certificate must match.
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Gets or sets the thumbprint for the client certificate.
        /// </summary>
        /// <remarks>
        /// If <see langword="null" /> only issuer will be checked.
        /// </remarks>
        public string Thumbprint { get; set; }

        /// <summary>
        /// Gets or sets the certificate that signed/issued to client certificate.
        /// </summary>
        public X509Certificate2 Certificate { get; set; }
    }
}