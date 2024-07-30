$outFolder="../Publish/AnomalousAdventure"
$outFolder=[System.IO.Path]::GetFullPath($outFolder)
$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path

$steamCmdLoc="..\SteamSdk\sdk\tools\ContentBuilder\builder\steamcmd.exe"
if ($null -eq $env:STEAM_PIPE_USER) 
{ 
	$env:STEAM_PIPE_USER = Read-Host "Please enter the steam user account to upload with."
}
if ($null -eq $env:STEAM_PIPE_PASS) 
{ 
	$env:STEAM_PIPE_PASS = Read-Host "Please enter the steam account password." -MaskInput
}

&"$steamCmdLoc" `
+login $env:STEAM_PIPE_USER $env:STEAM_PIPE_PASS `
+run_app_build "$scriptPath\Adventure\Steam\Playtest.vdf" `
+run_app_build "$scriptPath\Adventure\Steam\Demo.vdf" `
+run_app_build "$scriptPath\Adventure\Steam\Full.vdf" `
+quit