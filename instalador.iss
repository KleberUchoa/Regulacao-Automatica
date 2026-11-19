[Setup]
AppName=Regulação Automatizada
AppVersion=1.0.0
DefaultDirName={autopf}\Regulação Automatizada
DefaultGroupName=Regulação Automatizada
OutputDir=Output
OutputBaseFilename=Setup

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Regulação Automatizada"; Filename: "{app}\listar-exames.exe"

[Run]
Filename: "powershell.exe"; \
  Parameters: "-NoLogo -NoProfile -ExecutionPolicy Bypass -File ""{app}\playwright.ps1"" install"; \
  WorkingDir: "{app}"; \
  StatusMsg: "Instalando o navegador Chromium do Playwright..."; \
  Flags: waituntilterminated runhidden
Filename: "{app}\listar-exames.exe"; Flags: nowait postinstall skipifsilent