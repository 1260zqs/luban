@echo off

set publish_dir=..\..\Excel\_make\luban
REM if not exist %publish_dir% (
  REM echo output path dose not exist %publish_dir%
  REM pause
  REM goto :EOF
REM )

pushd src
dotnet publish Luban\Luban.csproj --configuration Release /p:DebugType=none -o %publish_dir%
popd

if %errorlevel% neq 0 pause