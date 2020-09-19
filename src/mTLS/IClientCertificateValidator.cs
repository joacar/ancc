using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace ancc.mTLS
{
    public interface IClientCertificateValidator
    {
        bool Validate(X509Certificate2 clientCertificate, X509Chain chain, SslPolicyErrors errors);
    }
}