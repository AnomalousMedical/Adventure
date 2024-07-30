msbuild.exe /m AdventureWindows.sln '/property:Configuration=Release;Platform=x64'
Push-Location Adventure
dotnet publish -r win-x64 -c Release
Pop-Location
$outFolder="../Publish/AnomalousAdventure"
$outFolder=[System.IO.Path]::GetFullPath($outFolder)
if (test-path $outFolder) {
	Remove-Item -Path $outFolder -Recurse -Force
}
New-Item -Path $outFolder -Type Directory -Force
Move-Item -Path Adventure/bin/Release/net8.0/win-x64/publish/* -Destination $outFolder -Exclude @("*.pdb", "steam_appid.txt")
New-Item -Path $outFolder/AdventureAssets -Type Directory -Force
Copy-Item -Path ../AdventureAssets/Fonts -Destination $outFolder/AdventureAssets -Exclude @(".git") -Recurse
Copy-Item -Path ../AdventureAssets/Graphics -Destination $outFolder/AdventureAssets -Exclude @(".git") -Recurse
Copy-Item -Path ../AdventureAssets/Music -Destination $outFolder/AdventureAssets -Exclude @(".git") -Recurse
Copy-Item -Path ../AdventureAssets/SoundEffects -Destination $outFolder/AdventureAssets -Exclude @(".git") -Recurse
"Project written to $outFolder"