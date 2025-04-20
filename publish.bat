pushd src
dotnet publish Luban\Luban.csproj --configuration Release /p:DebugType=none -o ..\..\Excel\tool
popd
pause