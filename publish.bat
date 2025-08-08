@echo off

set publish_dir=..\..\Excel\_make\luban
if not exist %publish_dir% (
  echo output path dose not exist %publish_dir%
  pause
  goto :EOF
)

pushd src
dotnet publish Luban\Luban.csproj --configuration Release /p:DebugType=none -o %publish_dir%
popd

if %errorlevel% neq 0 pause