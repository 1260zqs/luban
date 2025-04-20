@echo off

set publish_dir=..\..\Excel\_make\luban

pushd src
dotnet publish Luban\Luban.csproj --configuration Release /p:DebugType=none -o %publish_dir%
popd

if %errorlevel% neq 0 pause