## Context

사용자가 새로운 오디오 장치를 연결하거나 기존 장치의 세부 설정을 변경할 때, Windows의 최신 설정 앱보다 클래식 소리 제어판(`mmsys.cpl`)을 선호하는 경우가 많습니다. SoundSwitcher는 사운드 장치 관리를 위한 앱이므로, 앱 트레이 메뉴에서 직접 소리 제어판을 호출할 수 있으면 접근성이 향상됩니다.

## Goals / Non-Goals

**Goals:**
- 트레이 메뉴에 "시스템 소리 설정" 메뉴 항목 추가
- "시스템 소리 설정" 클릭 시 Windows의 기본 소리 제어판(`mmsys.cpl`) 실행

**Non-Goals:**
- SoundSwitcher 앱 내부에서 클래식 소리 제어판의 기능을 자체적으로 구현하거나 대체하는 것

## Decisions

1. **외부 프로세스 실행 방법**: `System.Diagnostics.Process.Start`를 사용하여 `mmsys.cpl`을 실행합니다. 
   - Windows 환경에서 `.cpl` 파일은 기본 연결 프로그램(`control.exe`)을 통해 실행되므로 셸 실행(`UseShellExecute = true`) 옵션이 필요합니다.
2. **트레이 메뉴 위치**: Avalonia 기반의 `TrayIcon.Menu` 내에서 '설정' 메뉴 바로 아래나 위에 "시스템 소리 설정" 항목을 추가하고, 다른 메뉴와 명확히 구분되도록 구분선(Separator)을 추가합니다.

## Risks / Trade-offs

- **Risk**: Windows 시스템 환경에 따라 `mmsys.cpl` 파일 경로를 찾지 못하거나 실행 권한 문제가 발생할 수 있습니다.
  - **Mitigation**: `try-catch` 블록으로 예외를 처리하고 필요한 경우 로거를 통해 오류를 남기거나 무시하도록 하여 애플리케이션 크래시를 방지합니다.
