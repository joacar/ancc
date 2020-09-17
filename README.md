# ASP.NET Core Certificate

Trying to get my head around mTLS and client certificate authentication.

## TODOS

* Instruct IIS Express to use server certificate and thus only accept client certificates issued by server certificate.

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