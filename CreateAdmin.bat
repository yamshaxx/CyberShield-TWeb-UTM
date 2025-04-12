@echo off
echo Setting up environment...
set MSBuildPath=C:\Windows\Microsoft.NET\Framework\v4.0.30319

echo Copying required DLLs...
mkdir bin\Debug 2>nul
copy CyberShieldWeb\bin\System.Web.Helpers.dll bin\Debug\ >nul
copy CyberShield.Domain\bin\Debug\CyberShield.Domain.dll bin\Debug\ >nul
copy CyberShield.Domain\bin\Debug\CyberShield.Helper.dll bin\Debug\ >nul
copy CyberShield.Domain\bin\Debug\EntityFramework.dll bin\Debug\ >nul
copy CyberShield.Domain\bin\Debug\EntityFramework.SqlServer.dll bin\Debug\ >nul

echo Compiling admin user creation utility...
%MSBuildPath%\csc.exe /out:bin\Debug\CreateAdminUser.exe /reference:bin\Debug\CyberShield.Domain.dll /reference:bin\Debug\System.Web.Helpers.dll /reference:bin\Debug\EntityFramework.dll /reference:bin\Debug\EntityFramework.SqlServer.dll /reference:System.ComponentModel.DataAnnotations.dll /target:exe CreateAdminUser.cs

echo Setting up database path...
set DataDirectory=%cd%\CyberShieldWeb\App_Data

echo Running admin user creation utility...
bin\Debug\CreateAdminUser.exe

echo Done.
pause 