:: =============================================================================
:: File         : dependency/proto/cpp_output.cmd
:: Description  : Generate C++ files from proto files on Windows
:: -----------------------------------------------------------------------------
:: Copyright    : (C) 2025 EESAST
:: License      : MIT License
::               See LICENSE file in the project root for full license text.
::               This file is part of EESAST/THUAI8 or later versions of
::               THUAI or teamstyle.
:: -----------------------------------------------------------------------------
:: First Created: 2025-04-22      Author: Timothy Liu Xuefeng
:: Last Modified: 2025-04-22      Author: Timothy Liu Xuefeng
:: =============================================================================

@ECHO OFF
SETLOCAL

SET "AUTO=0"
SET "SHOW_HELP=0"

IF "%1" EQU "/?" (
    SET "SHOW_HELP=1"
)

FOR %%I IN (%*) DO (
    IF /I "%%I" EQU "/A" SET "AUTO=1"
    IF /I "%%I" EQU "/-A" SET "AUTO=0"
    IF /I "%%I" EQU "/H" SET "SHOW_HELP=1"
)

IF "%SHOW_HELP%"=="1" (
    ECHO Usage: %~NX0 [/A] [/H or /?]
    ECHO.
    ECHO   /A       Automatically find protoc.exe and grpc_cpp_plugin.exe in PATH.
    ECHO   /-A      Skip automatically finding protoc.exe and grpc_cpp_plugin.exe.
    ECHO   /H       Show this help message.
    ECHO.
    ECHO   /-A is the default option if no arguments are provided.
    EXIT /B 0
)

SET "PROTOC_PATH="
SET "CPP_PLUGIN_PATH="

IF "%AUTO%" NEQ "1" (
    ECHO Auto mode is not enabled. Automatic findings will be skipped.
)

IF "%AUTO%" NEQ "1" (
    GOTO :ASK_PROTOC
)

:FIND_PROTOC
ECHO Finding protoc.exe in PATH...
FOR %%D IN ("%PATH:;=" "%") DO (
    IF EXIST "%%~D\protoc.exe" (
        SET "PROTOC_PATH=%%~D\protoc.exe"
        ECHO Found protoc.exe at %%~D\protoc.exe
        ECHO.
        GOTO :FOUND_PROTOC
    )
)
ECHO Failed to find protoc.exe automatically.
ECHO.

:ASK_PROTOC
SET /P "USER_INPUT=Please input the full path of PROTOC.exe (e.g. C:\Users\Administrator\vcpkg\installed\x64-windows\tools\protobuf\protoc.exe): "
IF EXIST "%USER_INPUT%" (
    SET "PROTOC_PATH=%USER_INPUT%"
    GOTO :FOUND_PROTOC
) ELSE (
    ECHO %USER_INPUT% does not exist, please try again.
    GOTO :ASK_PROTOC
)

:FOUND_PROTOC

IF "%AUTO%" NEQ "1" (
    GOTO :ASK_CPP_PLUGIN
)

:FIND_CPP_PLUGIN
ECHO Finding grpc_cpp_plugin.exe in PATH...
FOR %%D IN ("%PATH:;=" "%") DO (
    IF EXIST "%%~D\grpc_cpp_plugin.exe" (
        SET "CPP_PLUGIN_PATH=%%~D\grpc_cpp_plugin.exe"
        ECHO Found grpc_cpp_plugin.exe at %%~D\grpc_cpp_plugin.exe
        ECHO.
        GOTO :FOUND_CPP_PLUGIN
    )
)
ECHO Failed to find grpc_cpp_plugin.exe automatically.
ECHO.

:ASK_CPP_PLUGIN
SET /P "USER_INPUT=Please input the full path of grpc_cpp_plugin.exe (e.g. C:\Users\Administrator\vcpkg\installed\x64-windows\tools\grpc\grpc_cpp_plugin.exe): "
IF EXIST "%USER_INPUT%" (
    SET "CPP_PLUGIN_PATH=%USER_INPUT%"
    GOTO :FOUND_CPP_PLUGIN
) ELSE (
    ECHO %USER_INPUT% does not exist, please try again.
    GOTO :ASK_CPP_PLUGIN
)

:FOUND_CPP_PLUGIN

@ECHO ON

@ECHO.
@ECHO Generating C++ files from proto files...
@ECHO.

"%PROTOC_PATH%" Message2Clients.proto --cpp_out=.
@IF %ERRORLEVEL% NEQ 0 GOTO :ERROR
"%PROTOC_PATH%" MessageType.proto --cpp_out=.
@IF %ERRORLEVEL% NEQ 0 GOTO :ERROR
"%PROTOC_PATH%" Message2Server.proto --cpp_out=.
@IF %ERRORLEVEL% NEQ 0 GOTO :ERROR
"%PROTOC_PATH%" Services.proto --grpc_out=. --plugin=protoc-gen-grpc="%CPP_PLUGIN_PATH%"
@IF %ERRORLEVEL% NEQ 0 GOTO :ERROR
"%PROTOC_PATH%" Services.proto --cpp_out=.
@IF %ERRORLEVEL% NEQ 0 GOTO :ERROR

@ECHO.
@ECHO Successfully generated C++ files.
@ECHO.
@ECHO Moving generated files to CAPI\cpp\proto directory...
@ECHO.

MOVE /Y .\*.h ..\..\CAPI\cpp\proto
@IF %ERRORLEVEL% NEQ 0 GOTO :ERROR
MOVE /Y .\*.cc ..\..\CAPI\cpp\proto
@IF %ERRORLEVEL% NEQ 0 GOTO :ERROR

@ECHO.
@ECHO Successfully moved generated files to CAPI\cpp\proto directory.
@ECHO.

@ECHO OFF

GOTO :END

:ERROR
@ECHO OFF
SET "EXIT_CODE=%ERRORLEVEL%"
ECHO.
ECHO Failed to generate or move C++ files.
ECHO.
PAUSE
EXIT /B %EXIT_CODE%

:END
@ECHO OFF
ECHO.
ECHO Done.
ECHO.
PAUSE
EXIT /B 0
