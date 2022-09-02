@echo off
set sourceFile="C:\Temp\MauiAbpOpenIddict\MauiBookStore\src\MauiBookStore\appsettings.json"
set portNumber=44330

setlocal disabledelayedexpansion

start ngrok.exe http -region eu https://localhost:%portNumber%/

timeout 5 > NUL

if not exist "C:\TEMP_NGROK\" mkdir "C:\TEMP_NGROK\"

for /F %%I in ('curl -s http://127.0.0.1:4040/api/tunnels ^| jq -r .tunnels[0].public_url') do set ngrokTunnel=%%I
echo %ngrokTunnel%
echo %ngrokTunnel%/.well-known/openid-configuration

for /f "tokens=1* delims=]" %%a in ('find /n /v "" %sourceFile%') do (
  echo %%b|findstr /rc:"\ *\"AuthorityUrl\".*\:\ \".*\"" >nul && (
    for /f "delims=:" %%c in ("%%b") do echo %%c: "%ngrokTunnel%",
    ) || echo/%%b
)>>C:\TEMP_NGROK\temp.json 2>nul

for %%I in (C:\TEMP_NGROK\temp.json) do for /f "delims=, tokens=* skip=1" %%x in (%%I) do echo %%x >> "%%I.new"

move /Y "C:\TEMP_NGROK\temp.json.new" %sourceFile% > nul

del C:\TEMP_NGROK\temp.json
rmdir /s /q "C:\TEMP_NGROK\"


