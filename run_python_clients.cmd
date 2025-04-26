@echo off

start cmd /k python .\CAPI\python\PyAPI\main.py -I 127.0.0.1 -p 0 -t 0 -P 8888 -o -d -s 0
start cmd /k python .\CAPI\python\PyAPI\main.py -I 127.0.0.1 -p 0 -t 1 -P 8888 -o -d -s 1
start cmd /k python .\CAPI\python\PyAPI\main.py -I 127.0.0.1 -p 1 -t 0 -P 8888 -o -d -s 0
start cmd /k python .\CAPI\python\PyAPI\main.py -I 127.0.0.1 -p 1 -t 1 -P 8888 -o -d -s 1