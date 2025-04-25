start logic\Server\bin\Debug\net8.0\Server.exe --port 8888 --CharacterNum 1
timeout /t 2 /nobreak
start CAPI\cpp\x64\Debug\API.exe -p 0 -t 0 -P 8888 -o -d
timeout /t 0.5 /nobreak
start CAPI\cpp\x64\Debug\API.exe -p 0 -t 1 -P 8888 -o -d
timeout /t 0.5 /nobreak
start CAPI\cpp\x64\Debug\API.exe -p 1 -t 0 -P 8888 -o -d
timeout /t 0.5 /nobreak
start CAPI\cpp\x64\Debug\API.exe -p 1 -t 1 -P 8888 -o -d
