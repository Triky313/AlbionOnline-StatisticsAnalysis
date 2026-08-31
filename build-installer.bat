@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "BUILD_EXIT_CODE=0"
set "ROOT_DIRECTORY=%~dp0"
set "PROJECT_FILE=%ROOT_DIRECTORY%src\StatisticsAnalysisTool\StatisticsAnalysisTool.csproj"
set "ASSEMBLY_INFO=%ROOT_DIRECTORY%src\StatisticsAnalysisTool\Properties\AssemblyInfo.cs"
set "PUBLISH_DIRECTORY=%ROOT_DIRECTORY%src\StatisticsAnalysisTool\bin\Release\publish"
set "INSTALLER_SCRIPT=%ROOT_DIRECTORY%.github\installers\release-installer.iss"

pushd "%ROOT_DIRECTORY%" >nul

set "APP_VERSION=%~1"
if not defined APP_VERSION (
    for /f "tokens=2 delims=()" %%V in ('findstr /c:"AssemblyInformationalVersion(" "%ASSEMBLY_INFO%"') do (
        if not defined APP_VERSION set "APP_VERSION=%%~V"
    )
)

if not defined APP_VERSION (
    echo ERROR: The application version could not be read from AssemblyInfo.cs.
    set "BUILD_EXIT_CODE=1"
    goto :finish
)

if /i "%APP_VERSION:~0,1%"=="v" set "APP_VERSION=%APP_VERSION:~1%"
set "LOCAL_INSTALLER_VERSION=%APP_VERSION%"

powershell.exe -NoLogo -NoProfile -NonInteractive -Command "$version = $env:LOCAL_INSTALLER_VERSION; if ($version -notmatch '^\d+(?:\.\d+){0,3}(?:-(?:alpha|beta|rc)(?:[.-]?\d+(?:[.-]\d+)*)?)?$') { exit 1 }"
if errorlevel 1 (
    echo ERROR: Version "%APP_VERSION%" is invalid. Use a numeric version with an optional alpha, beta, or rc suffix.
    set "BUILD_EXIT_CODE=1"
    goto :finish
)
set "LOCAL_INSTALLER_VERSION="

set "NUMERIC_VERSION=%APP_VERSION%"
for /f "tokens=1 delims=-" %%V in ("%NUMERIC_VERSION%") do set "NUMERIC_VERSION=%%V"
set "VERSION_MAJOR=0"
set "VERSION_MINOR=0"
set "VERSION_BUILD=0"
set "VERSION_REVISION=0"
for /f "tokens=1-4 delims=." %%A in ("%NUMERIC_VERSION%") do (
    set "VERSION_MAJOR=%%A"
    if not "%%B"=="" set "VERSION_MINOR=%%B"
    if not "%%C"=="" set "VERSION_BUILD=%%C"
    if not "%%D"=="" set "VERSION_REVISION=%%D"
)
set "APP_VERSION_INFO=%VERSION_MAJOR%.%VERSION_MINOR%.%VERSION_BUILD%.%VERSION_REVISION%"

where dotnet.exe >nul 2>&1
if errorlevel 1 (
    echo ERROR: The .NET SDK was not found. Install the SDK configured in src\global.json.
    set "BUILD_EXIT_CODE=1"
    goto :finish
)

set "ISCC_PATH="
for /f "delims=" %%I in ('where ISCC.exe 2^>nul') do if not defined ISCC_PATH set "ISCC_PATH=%%I"
if not defined ISCC_PATH if exist "%LocalAppData%\Programs\Inno Setup 6\ISCC.exe" set "ISCC_PATH=%LocalAppData%\Programs\Inno Setup 6\ISCC.exe"
if not defined ISCC_PATH if exist "%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe" set "ISCC_PATH=%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe"
if not defined ISCC_PATH if exist "%ProgramFiles%\Inno Setup 6\ISCC.exe" set "ISCC_PATH=%ProgramFiles%\Inno Setup 6\ISCC.exe"

if not defined ISCC_PATH (
    echo ERROR: The Inno Setup compiler ISCC.exe was not found.
    echo Install Inno Setup 6 or add its installation directory to PATH.
    set "BUILD_EXIT_CODE=1"
    goto :finish
)

set "OUTPUT_FILE=%ROOT_DIRECTORY%StatisticsAnalysis-AlbionOnline-v%APP_VERSION%-windows-x64.exe"

echo Publishing Statistics Analysis Tool %APP_VERSION%...
if exist "%PUBLISH_DIRECTORY%" rmdir /s /q "%PUBLISH_DIRECTORY%"
if exist "%PUBLISH_DIRECTORY%" (
    echo ERROR: The previous publish directory could not be removed.
    set "BUILD_EXIT_CODE=1"
    goto :finish
)

dotnet publish "%PROJECT_FILE%" ^
    -c Release ^
    -p:Platform=x64 ^
    -p:PublishDir=bin\Release\publish ^
    -p:PublishProtocol=FileSystem ^
    -p:TargetFramework=net10.0-windows10.0.19041.0 ^
    -p:RuntimeIdentifier=win-x64 ^
    -p:SelfContained=false ^
    -p:PublishSingleFile=true ^
    -p:PublishReadyToRun=false ^
    -p:LocalInstallerBuild=true
if errorlevel 1 (
    echo ERROR: Publishing the application failed.
    set "BUILD_EXIT_CODE=1"
    goto :finish
)

if exist "%OUTPUT_FILE%" del /q "%OUTPUT_FILE%"
if exist "%OUTPUT_FILE%" (
    echo ERROR: The previous installer could not be removed.
    set "BUILD_EXIT_CODE=1"
    goto :finish
)

echo Building installer...
"%ISCC_PATH%" "/DMyAppVersion=%APP_VERSION%" "/DMyAppVersionInfo=%APP_VERSION_INFO%" "%INSTALLER_SCRIPT%"
if errorlevel 1 (
    echo ERROR: Building the installer failed.
    set "BUILD_EXIT_CODE=1"
    goto :finish
)

if not exist "%OUTPUT_FILE%" (
    echo ERROR: Inno Setup completed without creating the expected installer.
    set "BUILD_EXIT_CODE=1"
    goto :finish
)

echo Installer created successfully:
echo %OUTPUT_FILE%
echo NOTE: Automatic update checks are disabled for this local installer build.
echo NOTE: Updates requested manually are not signature-verified.

:finish
popd >nul
endlocal & exit /b %BUILD_EXIT_CODE%
