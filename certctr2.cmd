@ECHO off

SETLOCAL EnableDelayedExpansion

REM Check if makecert is in path
REM Example: "C:\Program Files (x86)\Windows Kits\10\bin\10.0.17763.0\x86\makecert.exe"
where /Q "$path:makecert"
IF "%ERRORLEVEL%" NEQ "0" (
  ECHO "[ERROR:%~nx0] Could not found makecert in path. Launch VS Developer Command Prompt or add makecert to path"
  EXIT /B
)

SET PWD=P4ssword
ECHO Generating pfx file require password otherwise it will fail when imported to store
ECHO with "Password doesn't match". Using password '%PWD%'

SET ROOT_SUFFIX=_CARoot

IF /I "%1" == "root" (
	GOTO :ROOT
)
IF /I "%1" == "server" (
	GOTO :SERVER
)
IF /I "%1" == "client" (
	GOTO :CLIENT
)

SET /P bn="Base name for certificate names (CN) and file names: "

SET root=%bn%%ROOT_SUFFIX%

:ROOT
ECHO [ROOT] Creating certificate ...
makecert -n "CN=localhost" -a sha512 -r -pe -cy authority -sv %root%.pvk %root%.cer
REM -sr localmachine -ss root 

IF "%ERRORLEVEL%" NEQ "0" (
	EXIT /B %ERRORLEVEL%
)

ECHO Import %root%.cer to Local Computer\Trusted Root Certification Authorities (certlm.exe)

echo [ROOT] Creating .pfx file ...
pvk2pfx -pvk %root%.pvk -pi %PWD% -spc %root%.cer -pfx %root%.pfx -po %PWD%

REM ECHO [ROOT] Creating revocation list ...
REM makecert -crl -n %CN% -r -sv %pvk% %root%.crl

IF /I "%1" == "root" (
	GOTO :END
)

:SERVER
IF [%bn%] == [] (
	SET /P bn="Base name for certificate names (CN) and file names: "
)
SET server=%bn%_server
SET root=%bn%%ROOT_SUFFIX%

REM makecert -n “CN=CARoot Sub” -iv CARoot.pvk -ic CARoot.cer -pe -a sha512 -len 4096 -cy authority -sv SCARoot.pvk SCARoot.cer
REM -sr LocalMachine -ss Root ****Optional parameters
REM pvk2pfx -pvk SCARoot.pvk -spc SCARoot.cer -pfx SCARoot.pfx

ECHO [SERVER] Creating certificate ...
REM 1.3.6.1.5.5.7.3.1 means Server Certificate
makecert.exe -n "CN=localhost" -iv %root%.pvk -ic %root%.cer -pe -a sha512 ^
-sky exchange -eku 1.3.6.1.5.5.7.3.1 -sv %server%.pvk %server%.cer

IF "%ERRORLEVEL%" NEQ "0" (
	EXIT /B %ERRORLEVEL%
)

REM -sr localmachine -ss my Require admin
ECHO Import %server%.cer to Local Computer\Personal (certlm.exe)

ECHO [SERVER] Creating .pfx file ...
pvk2pfx.exe -pvk %server%.pvk -pi %PWD% -spc %server%.cer -pfx %server%.pfx -po %PWD%

ECHO [SERVER] Copy pfx to %APPDATA%\ASP.NET\https\
xcopy /F /-Y  %server%.pfx "%APPDATA%\ASP.NET\https\"

ECHO [SERVER] Creating revocation list ...
makecert -crl -n "CN=localhost" -a sha512 -r -sv %server%.pvk %server%.crl

IF /I "%1" == "server" (
	GOTO :END
)

:CLIENT
IF [%bn%] == [] (
	SET /P bn="Base name for certificate names (CN) and file names: "
)
SET client=%bn%_client
SET root=%bn%%ROOT_SUFFIX%

ECHO [CLIENT] Creating certificate ...
REM 1.3.6.1.5.5.7.3.2 means Client Certificate
makecert.exe -n "CN=localhost" -a sha512 -iv %root%.pvk -ic %root%.cer -pe ^
-sky exchange -eku 1.3.6.1.5.5.7.3.2 -sv %client%.pvk ^
-sr currentuser -ss my %client%.cer

ECHO [CLIENT] Creating .pfx file ...
pvk2pfx.exe -pvk %client%.pvk -pi %PWD% -spc %client%.cer -pfx %client%.pfx -po %PWD%

:END
echo Created files:
DIR /B *.pvk *.cer *.pfx *.crl