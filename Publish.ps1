msbuild.exe /m AdventureWindows.sln '/property:Configuration=Release;Platform=x64'
Push-Location Adventure
dotnet publish -r win-x64 -c Release
Pop-Location