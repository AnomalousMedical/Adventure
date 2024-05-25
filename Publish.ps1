msbuild.exe /m AdventureWindows.sln '/property:Configuration=Release;Platform=x64'
Push-Location Adventure
dotnet publish -r win-x64 -c Release
Pop-Location
$outFolder="../Publish/AnomalousAdventure"
if (test-path $outFolder) {
	Remove-Item -Path $outFolder -Recurse -Force
}
New-Item -Path $outFolder -Type Directory -Force
Move-Item -Path Adventure/bin/Release/net8.0/win-x64/publish/* -Destination $outFolder -Exclude @("*.pdb")
New-Item -Path $outFolder/AdventureAssets -Type Directory -Force
Copy-Item -Path ../AdventureAssets/* -Destination $outFolder/AdventureAssets -Exclude @(".git") -Recurse
"Project written to $outFolder"