@ECHO off
SETLOCAL

REM Thanks to:
REM http://www.yangsoft.com/blog/?p=105
REM https://docs.microsoft.com/en-us/previous-versions/msp-n-p/ff650751(v=pandp.10)?redirectedfrom=MSDN

REM Check if makecert is in path
REM Example: "C:\Program Files (x86)\Windows Kits\10\bin\10.0.17763.0\x86\makecert.exe"
where /Q "$path:makecert"
IF "%ERRORLEVEL%" NEQ "0" (
  ECHO "[ERROR:%~nx0] Could not found makecert in path. Launch VS Developer Command Prompt or add makecert to path"
  EXIT /B
)

SET /P name="Enter name for CN and files .pvk and .cer: "
SET CN="CN=%name%"
SET pvk=%name%.pvk
SET cer=%name%.cer

ECHO Creating certificates %CN% ...
makecert -n %CN% -r -sv %pvk% %cer%

REM If run as admin then we should be able to add on the fly
REM certlc /add /c %cer% /s RootAuth /l LocalMachine


IF "%ERRORLEVEL%" NEQ "0" (
  ECHO Failed to generate certificates
  EXIT /B
)

ECHO Creating revocation list ...
makecert -crl -n %CN% -r -sv %pvk% %name%.crl

REM certlc /add /c %name%.crl /s RootAuth /l LocalMachine

IF "%ERRORLEVEL%" NEQ "0" (
  ECHO Failed to generate revocation list
  EXIT /B
)

SET /P key="Enter name for client certificate: "
ECHO Generating client certificate. This should be imported to User certificate store
makecert -sk %key% -iv %pvk% -n "CN=%key%" -ic %cer% -sr currentuser -ss my -sky signature -pe


echo Generate .pfx file ...
pvk2pfx -pvk %pvk% -spc %cer% -pfx %name%.pfx

echo Generated files:
DIR /B *.pvk *.cer *.pfx *.crl