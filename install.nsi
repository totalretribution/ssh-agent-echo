; Simple SSH Agent Echo Installer
!include "MUI2.nsh"

!ifndef VERSION
  !define VERSION "debug"
!endif

Name "SSH Agent Echo"
OutFile "publish\SshAgentEcho-Installer-${VERSION}.exe"
InstallDir "$LOCALAPPDATA\SshAgentEcho"
Icon "Assets\icon.ico"
UninstallIcon "Assets\icon.ico"

!define MUI_FINISHPAGE_RUN "$INSTDIR\ssh-agent-echo-gui.exe"
!define MUI_FINISHPAGE_RUN_CHECKED

!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_LANGUAGE "English"

Section "Install"
  SetOutPath "$INSTDIR"
  File "publish\ssh-agent-echo.exe"
  File "publish\ssh-agent-echo-gui.exe"
  
  WriteUninstaller "$INSTDIR\uninstall.exe"
  
  CreateDirectory "$SMPROGRAMS\SSH Agent Echo"
  CreateShortCut "$SMPROGRAMS\SSH Agent Echo\SSH Agent Echo.lnk" "$INSTDIR\ssh-agent-echo-gui.exe"
  CreateShortCut "$SMPROGRAMS\SSH Agent Echo\Uninstall.lnk" "$INSTDIR\uninstall.exe"
  
  ReadRegStr $0 HKCU "Environment" "Path"
  StrCpy $1 "$0;$INSTDIR"
  WriteRegStr HKCU "Environment" "Path" "$1"
  SendMessage 0xffff 0x001A 0 "STR:Environment" /TIMEOUT=5000
SectionEnd

Section "Uninstall"
  RMDir /r "$INSTDIR"
  RMDir /r "$SMPROGRAMS\SSH Agent Echo"
  
  ReadRegStr $0 HKCU "Environment" "Path"
  WriteRegStr HKCU "Environment" "Path" "$0"
  SendMessage 0xffff 0x001A 0 "STR:Environment" /TIMEOUT=5000
SectionEnd
