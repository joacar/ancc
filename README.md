# ASP.NET Core Certificate

Trying to get my head around mTLS and client certificate authentication.

Mutual TLS

1. Application owner generates a root certificate
1. Generated root certificate is used to generate a server certificate
1. Generated root certificate is used to generate a client certificate
1. Client certificate is sent to client to grant access to application

Could it possibly be used like this
1. Application owner generates a root certificate
1. Generated root certificate is used to generate a server certificate
1. Client sends a certificate to be signed by root certificate
1. Signed client certificate is used to grant access to application

## TODOS

* Instruct IIS Express to use server certificate and thus only accept client certificates issued by server certificate.
* Instruct Kestrel only accept client certificates signed by/issued by server certificate. Is this m-TLS?

## Observations

* `CertificateAuthenticationEvents.OnCertificateValidated` is not called if `app.UseAuthentication()` is missing.
* If revocation check fails the user can navigate to Home.

## Certificates

1. Run file `certcrt.cmd` and follow the instructions
1. Import client certificate to `Current User\Personal`. This is usually done during creation.
   1. Windows+R
   1. Type certmgr.exe then enter
   1. Right click Personal
   1. All Tasks > Import
   1. Select the generated `.cer` client file
1. Import server certificate to `Local Computer\Trusted Root Certitificates Authorities`
   1. Windows+R
   1. Type certlm.exe then Ctrl+Shift+Enter (start as admin)
   1. Right click Trusted Root Certitificates Authorities
   1. All Tasks > Import
   1. Select the generated `.cer` file
1. Import certificate revocation list. Same as above but select `.crl` file

## Kestrel

Follow the steps in [Application](#Application) to setup authentication.

The certificate and password are read from environment variables and I've created a function to read them and create the certificate used for the server.

```c#
private static X509Certificate2 CreateServerCertificate()
{
    // Error handling removed for brevity
    const string envFilePath = "ASPNETCORE_Kestrel__Certificates__Default__Path";
    const string envPassword = "ASPNETCORE_Kestrel__Certificates__Default__Password";
    var cert = var password = Environment.GetEnvironmentVariable(envFilePath);
    var password = Environment.GetEnvironmentVariable(envPassword);
    return new X509Certificate2(cert, password);
}
```

We need to configure Kestrel to request client certificate and we must provide a server certificate used to validate the client certificate. This is configured when the host is built, in this case in Program.cs.

```c#
webBuilder
    .UseStartup<Startup>()
    .ConfigureKestrel(options =>
    {
        options.ConfigureHttpsDefaults(https =>
        {
            https.ClientCertificateValidation += (certificate2, chain, errors) =>
            {
                // Perform any checks here
                return true;
            };
            https.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
            https.ServerCertificate = CreateServerCertificate();
        });
    });
```

Is the above the mTLS part? Then the certificate authentication might not be needed if another security scheme is employed, example OAuth2 Client Credentials grant with bearer token. I would like to see that only client certificates signed by/issued by the server certificate are allowed to connect.

### Observations

* Should Kestrel configure server certificate automatically if present with values in `ASPNETCORE_Kestrel__Certificates__Default__{Path,Password}`?
* No options for client certificate, example `ASPNETCORE_Kestrel__Client_Certificates__Default__`
* How to ensure that only client certificates signed by/issued by server certificate are allowed?

### Issues

> The specified network password is not correct

## IIS/IIS Express

Run file `iis.cmd` to update relevant configuration sections. The section `iisClientCertificateMappingAuthentication` must be enabled and the section `access` should have `sslFlags` set to "Ssl, SslNegotiateCert, SslRequireCert"`.

## Application

1. Add `Microsoft.AspNetCore.Authentication.Certificate` nuget package
1. Configure authentication protocol

   ```c#
   services
        .AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
        .AddCertificate(options =>
        {
            options.AllowedCertificateTypes = CertificateTypes.All;
            options.Events = new CertificateAuthenticationEvents
            {
                OnCertificateValidated = context =>
                {
                    // Do validation on context.Certificate here
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {

                    return Task.CompletedTask;
                }
            };
        });
   ```
1. Add `app.UseAuthentication()` before `app.UseAuthorization()` to hook into `CertificateAuthenticationEvents.OnCertificateValidated`. This is never called otherwise, leaving open for any certificate.

## ISSUES

> HTTP Error 403.16 - Forbidden
> Your client certificate is either not trusted or is invalid.

See [certificates](#Certificates) step 2.

> Warning: Certificate validation failed, subject was CN=ancc_client.
> RevocationStatusUnknown The revocation function was unable to check revocation for the certificate.

See [certificates](#Certificates) step 3 or disable recovation check `options.RevocationMode = X509RevocationMode.NoCheck`

## Thanks to

* http://www.yangsoft.com/blog/?p=105 - certctr.cmd
* https://docs.microsoft.com/en-us/previous-versions/msp-n-p/ff650751(v=pandp.10)?redirectedfrom=MSDN - certctr.cmd
* https://blog.jayway.com/2014/09/03/creating-self-signed-certificates-with-makecert-exe-for-development/ - certctr2.cmd