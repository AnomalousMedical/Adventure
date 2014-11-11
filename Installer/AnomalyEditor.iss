; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Anomaly Editor"
#define MyAppVersion GetFileVersion("S:\Engine\Release\Anomaly.exe")
#define MyAppPublisher "Anomalous Software"
#define MyAppURL "http://www.anomalousmedical.com"
#define MyAppExeName "Anomaly.exe"

#if Exec('S:\DRM\CodeKey\SignEditor.bat') != 0
#error Could not sign
#endif

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{DA0C94F1-997E-4315-95B4-E9B8E63E2531}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\Anomalous Software\Anomaly
DefaultGroupName=Anomalous Software
LicenseFile=S:\Medical\AnomalousMedical\Installer\License\en.rtf
OutputDir=S:\Engine\Release\Setups
OutputBaseFilename=AnomalySetup
Compression=lzma
SolidCompression=yes
SignTool=AnomalousMedicalSign $qAnomaly Editor$q $f
ChangesAssociations=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: S:\Engine\Release\Anomaly.exe; DestDir: {app}; Flags: ignoreversion 
Source: S:\Engine\Release\ImageAtlasPacker.exe; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\OgreModelEditor.exe; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\BEPUikPlugin.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\BulletPlugin.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\BulletWrapper.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\DotNetZip.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\Editor.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\Engine.dll; DestDir: {app}; Flags: ignoreversion
;Source: S:\Engine\Release\FreeImage.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\FreeImageNET.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\libRocketPlugin.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\libRocketWrapper.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\MyGUIPlugin.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\MyGUIWrapper.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\OgreCWrapper.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\OgreMain.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\OgrePlugin.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\OIS.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\OpenAL32.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\PCPlatform.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\PCPlatformPlugin.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\RenderSystem_Direct3D11.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\RenderSystem_GL.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\ShapeLoader.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\SoundPlugin.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\SoundWrapper.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\WeifenLuo.WinFormsUI.Docking.dll; DestDir: {app}; Flags: ignoreversion
Source: S:\Engine\Release\Zip.dll; DestDir: {app}; Flags: ignoreversion

;Microcode Caches
;Source: S:\Medical\Release\Direct3D11 Rendering Subsystem.mcc; DestDir: {app}; Flags: ignoreversion
;Source: S:\Medical\Release\OpenGL Rendering Subsystem.mcc; DestDir: {app}; Flags: ignoreversion

;VS 2013 Redistributable
Source: "S:\dependencies\InstallerDependencies\Windows\vcredist_x86.exe"; DestDir: "{tmp}"; Flags: ignoreversion deleteafterinstall

;.Net 4.5.1
Source: S:\dependencies\InstallerDependencies\Windows\NDP451-KB2859818-Web.exe; DestDir: {tmp}; 

;DX Required Files
Source: "S:\dependencies\InstallerDependencies\Windows\DirectX9c\DXSETUP.exe"; DestDir: "{tmp}\DirectX9c"; Flags: ignoreversion deleteafterinstall
Source: "S:\dependencies\InstallerDependencies\Windows\DirectX9c\dsetup32.dll"; DestDir: "{tmp}\DirectX9c"; Flags: ignoreversion deleteafterinstall
Source: "S:\dependencies\InstallerDependencies\Windows\DirectX9c\DSETUP.dll"; DestDir: "{tmp}\DirectX9c"; Flags: ignoreversion deleteafterinstall
Source: "S:\dependencies\InstallerDependencies\Windows\DirectX9c\dxdllreg_x86.cab"; DestDir: "{tmp}\DirectX9c"; Flags: ignoreversion deleteafterinstall
Source: "S:\dependencies\InstallerDependencies\Windows\DirectX9c\dxupdate.cab"; DestDir: "{tmp}\DirectX9c"; Flags: ignoreversion deleteafterinstall
;DX June 2010 Files
Source: "S:\dependencies\InstallerDependencies\Windows\DirectX9c\Jun2010_D3DCompiler_43_x86.cab"; DestDir: "{tmp}\DirectX9c"; Flags: ignoreversion deleteafterinstall

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Ogre Model Editor"; Filename: "{app}\OgreModelEditor.exe"
Name: "{group}\Image Atlas Packer"; Filename: "{app}\ImageAtlasPacker.exe"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{tmp}\vcredist_x86.exe"; Parameters: "/q /norestart"; StatusMsg: "Installing Visual Studio 2013 Redistributable (x86)";
Filename: "{tmp}\DirectX9c\DXSETUP.exe"; Parameters: "/silent"; StatusMsg: "Installing DirectX";
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, "&", "&&")}}"; Flags: nowait postinstall skipifsilent

[Registry]
Root: HKCR; Subkey: ".mesh"; ValueType: string; ValueName: ""; ValueData: "OgreMeshFiles"; Flags: uninsdeletevalue 
Root: HKCR; Subkey: "OgreMeshFiles"; ValueType: string; ValueName: ""; ValueData: "Ogre Mesh File"; Flags: uninsdeletekey 
Root: HKCR; Subkey: "OgreMeshFiles\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\OgreModelEditor.exe,0" 
Root: HKCR; Subkey: "OgreMeshFiles\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\OgreModelEditor.exe"" ""%1""" 

[Code]
procedure checkdotnetfx4();
var
  resultCode: Integer;
	release: cardinal;
begin
	RegQueryDWordValue(HKLM, 'Software\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', release);
	if (release < 378759) and (release <> 378675) and (release <> 378758) then 
	begin
    if MsgBox('You need to install the Microsoft .Net Framework 4.5.1.'#13#10'If you are connected to the internet you can do this now.'#13#10'Would you like to continue?', mbConfirmation, MB_YESNO) = IDYES then
      begin
        Exec(ExpandConstant('{tmp}\NDP451-KB2859818-Web.exe'), '/norestart', '', SW_SHOW, ewWaitUntilTerminated, resultCode);
      end
    else
      begin
        MsgBox('You must install the Microsoft .Net Framework 4.5.1 for this program to work.'#13#10'Please visit www.anomalousmedical.com for more info.', mbInformation, MB_OK)
      end;
   end;
//	   if resultCode=3010 then
	   //Restart required
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if(CurStep = ssPostInstall) then
    checkdotnetfx4();
end;
