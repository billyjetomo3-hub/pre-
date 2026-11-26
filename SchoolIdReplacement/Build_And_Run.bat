@echo off
title School ID Replacement System - Build & Launch
echo =====================================================================
echo  SCHOOL ID REPLACEMENT SYSTEM - PORTABLE BUILDER & LAUNCHER
echo =====================================================================
echo.

set "OUT_DIR=%~dp0SchoolIdReplacement\bin\Release"
if not exist "%OUT_DIR%" mkdir "%OUT_DIR%"

set "CSC="
if exist "%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe" set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if "%CSC%"=="" if exist "%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"

if "%CSC%"=="" (
    echo [ERROR] .NET Framework 4.x compiler (csc.exe) was not found on this PC.
    pause
    exit /b 1
)

echo [1/3] Compiling C# Windows Forms application...
"%CSC%" /nologo /target:winexe /optimize+ /platform:anycpu ^
  /out:"%OUT_DIR%\SchoolIdReplacement.exe" ^
  /r:System.dll ^
  /r:System.Configuration.dll ^
  /r:System.Core.dll ^
  /r:System.Data.dll ^
  /r:System.Data.DataSetExtensions.dll ^
  /r:System.Drawing.dll ^
  /r:System.Windows.Forms.dll ^
  /r:System.Xml.dll ^
  "%~dp0SchoolIdReplacement\Program.cs" ^
  "%~dp0SchoolIdReplacement\Properties\AssemblyInfo.cs" ^
  "%~dp0SchoolIdReplacement\Data\*.cs" ^
  "%~dp0SchoolIdReplacement\Forms\*.cs" ^
  "%~dp0SchoolIdReplacement\Models\*.cs" ^
  "%~dp0SchoolIdReplacement\Security\*.cs" ^
  "%~dp0SchoolIdReplacement\Services\*.cs" ^
  "%~dp0SchoolIdReplacement\UI\*.cs" ^
  "%~dp0SchoolIdReplacement\Views\*.cs"

if errorlevel 1 (
    echo.
    echo [ERROR] Compilation failed. Please review the errors above.
    pause
    exit /b 1
)

echo [2/3] Copying configuration file...
copy /Y "%~dp0SchoolIdReplacement\App.config" "%OUT_DIR%\SchoolIdReplacement.exe.config" >nul

echo [3/3] Launching SchoolIdReplacement.exe...
start "" "%OUT_DIR%\SchoolIdReplacement.exe"
echo.
echo Done! You can also share the entire "%OUT_DIR%" folder to run directly on any PC.
