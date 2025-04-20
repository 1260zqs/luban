@echo off
pushd src
dotnet publish Luban\Luban.csproj --configuration Release /p:DebugType=none -o ..\..\Excel\tool
popd

if %errorlevel% neq 0 pause