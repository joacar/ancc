@ECHO OFF

SETLOCAL

WHERE /Q appcmd.exe
IF %ERRORLEVEL% EQU 0 (
  ECHO "Found appcmd in path"
  goto :UNLOCK
)

SET appcmd="%systemroot%\system32\inetsrv\appcmd.exe"
ECHO Searching in %appcmd%
IF EXIST %appcmd% GOTO :UNLOCK

SET appcmd="%programFiles%\IIS Express\appcmd.exe"
ECHO Searching in %appcmd%
IF EXIST %appcmd% GOTO :UNLOCK

ECHO "Couldn't find appcmd.exe"
EXIT /B

:UNLOCK
SET CFG="%~dp0%.vs\ancc\config\applicationhost.config"
ECHO Using config file %CFG%

%appcmd% set config /apphostconfig:%CFG% /section:iisClientCertificateMappingAuthentication /enabled:true
%appcmd% set config /apphostconfig:%CFG% /section:security/access /sslFlags:"Ssl, SslNegotiateCert, SslRequireCert"

ENDLOCAL