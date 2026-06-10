# Capability: cswin32-interop

## Purpose
TBD - Provides type-safe access to Win32 APIs and COM interfaces via CsWin32

## Requirements

### Requirement: CsWin32 기반 Win32 API 호출 구조화
시스템은 Win32 API 및 COM 인터페이스 호출을 수동 P/Invoke 대신 `CsWin32` 소스 생성기를 통해 생성된 코드로 수행해야 한다.

#### Scenario: 컴파일 타임 API 생성
- **WHEN** 개발자가 `NativeMethods.txt`에 필요한 Win32 API (예: `RegisterHotKey`)를 추가하고 빌드할 때
- **THEN** 시스템은 `Windows.Win32.PInvoke` 네임스페이스 하위에 해당 API의 타입-세이프한 C# 서명을 자동 생성한다.

#### Scenario: 런타임 Win32 API 호출
- **WHEN** 애플리케이션이 글로벌 단축키 등록, 오디오 장치 제어 등 운영체제 기능을 요청할 때
- **THEN** 시스템은 `CsWin32`로 자동 생성된 P/Invoke 메서드를 통해 정상적으로 운영체제와 상호작용한다.
