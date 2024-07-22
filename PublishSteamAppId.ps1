$outFolder="../Publish/AnomalousAdventure"
$outFolder=[System.IO.Path]::GetFullPath($outFolder)
cp Adventure/steam_appid.txt -Destination $outFolder/steam_appid.txt
"steam_appid.txt written to $outFolder"