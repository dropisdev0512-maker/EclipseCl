@echo off
setlocal enabledelayedexpansion

echo ============================================
echo   Eclipse Client - Build Script
echo ============================================
echo.

cd /d "%~dp0"

echo [1/3] Generating icons...
powershell -ExecutionPolicy Bypass -File "scripts\GenerateIcons.ps1"
if errorlevel 1 (
    echo Warning: Icon generation failed. App will use fallback icons.
)

echo.
echo [2/3] Building WPF application...
dotnet build src\EclipseClient\EclipseClient.csproj -c Release
if errorlevel 1 (
    echo ERROR: Build failed!
    exit /b 1
)

echo.
echo [3/3] Building native DLL...
set DLL_OUT=src\EclipseClient\bin\Release\net8.0-windows\native
mkdir "%DLL_OUT%" 2>nul

where cl >nul 2>&1
if %errorlevel%==0 (
    pushd native\EclipseCore
    cl /LD /EHsc /O2 dllmain.cpp /Fe:EclipseCore.dll /link /DEF:NUL
    if exist EclipseCore.dll (
        copy /Y EclipseCore.dll "..\..\%DLL_OUT%\"
        echo Native DLL built successfully.
    )
    popd
) else (
    echo MSVC compiler not found. Skipping native DLL build.
    echo Install "Desktop development with C++" workload or run from Developer Command Prompt.
    echo The Inject button requires EclipseCore.dll in the native\ folder.
)

echo.
echo ============================================
echo   Build Complete!
echo   Output: src\EclipseClient\bin\Release\net8.0-windows\EclipseClient.exe
echo ============================================
echo.
echo Default login:
echo   Email: dropisnotdev0512@gmail.com
echo   Password: anas@drop007
echo.

endlocal
