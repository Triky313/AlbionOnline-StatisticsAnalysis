#define MyAppName "Statistics Analysis Tool"
#define MyAppPublisher "Aaron Schultz"
#define MyAppExeName "StatisticsAnalysisTool.exe"
#define MyAppId "{{6D7ED979-FC39-4D6A-83A4-6493E2C61A16}}"

#ifndef MyAppVersion
  #define MyAppVersion "0.0.0"
#endif

[Setup]
AppId={#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL=https://github.com/Triky313/AlbionOnline-StatisticsAnalysis
DefaultDirName={autopf}\Statistics Analysis Tool
DefaultGroupName={#MyAppName}
UsePreviousAppDir=no
DisableDirPage=no
DisableProgramGroupPage=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
Compression=lzma
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
CloseApplicationsFilter={#MyAppExeName}
RestartApplications=no
OutputDir=..\..
OutputBaseFilename=StatisticsAnalysis-AlbionOnline-v{#MyAppVersion}-windows-x64
SetupIconFile=..\..\src\StatisticsAnalysisTool\sat-icon.ico
VersionInfoVersion={#MyAppVersion}
VersionInfoProductVersion={#MyAppVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Dirs]
Name: "{app}"; Permissions: users-modify
Name: "{app}\UserData"; Permissions: users-modify; Flags: uninsneveruninstall

[Files]
Source: "..\..\src\StatisticsAnalysisTool\bin\Release\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
