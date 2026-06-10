## Context

현재 `SoundSwitcher` 애플리케이션은 오디오 장치 제어 및 글로벌 단축키 등록 등 Windows 운영체제와의 상호작용을 위해 수동으로 P/Invoke(`DllImport`, `LibraryImport`)를 선언하여 사용하고 있습니다. 수동 선언은 오류가 발생하기 쉽고, 서명 변경 시 유지관리가 어려우며, 상수나 구조체 정의를 일일이 찾아 적어야 하는 번거로움이 있습니다. 이를 해결하기 위해 Microsoft에서 공식 지원하는 소스 생성기인 `CsWin32`를 도입하여, `NativeMethods.txt`에 필요한 API 이름만 작성하면 컴파일 타임에 타입-세이프(Type-safe)하고 최적화된 P/Invoke 코드를 자동으로 생성하도록 변경합니다.

## Goals / Non-Goals

**Goals:**
- 수동으로 작성된 모든 Win32 API 선언(메서드, 구조체, 열거형, 상수)을 `CsWin32`로 대체
- `Microsoft.Windows.CsWin32` 패키지 설치 및 구성 (`NativeMethods.txt`)
- 코드 전반에서 생성된 코드(`Windows.Win32.PInvoke` 등)를 사용하도록 리팩터링하여 안정성 및 가독성 향상

**Non-Goals:**
- Win32 API를 사용하지 않는 일반적인 비즈니스 로직 및 UI의 구조적 변경
- 오디오 디바이스 제어 방식 자체의 재설계 (단순히 호출 방식만 변경)
- 새로운 기능 추가

## Decisions

1. **CsWin32 소스 생성기 사용**: `CsWin32`는 공식적이고 지속적으로 업데이트되는 Win32 메타데이터를 기반으로 코드를 생성하므로 호환성과 성능이 우수합니다.
   - *대안*: `TerraFX.Interop.Windows` (C# 10 이상에서 잘 동작하지만 코드가 방대해질 수 있음) → `CsWin32`가 개별 API 선택적 생성(`NativeMethods.txt`)을 지원하여 앱 크기와 컴파일 타임 측면에서 더 유리함.
2. **NativeMethods.txt 구성**: 기존에 사용되던 API 목록(예: `RegisterHotKey`, `UnregisterHotKey`, `SendInput`, CoreAudio API 인터페이스 등)을 수집하여 `NativeMethods.txt`에 명시합니다.
3. **COM 인터페이스 처리**: CoreAudio API(예: `IMMDeviceEnumerator`)와 같은 COM 인터페이스도 `CsWin32`를 통해 생성된 인터페이스를 사용하도록 변경합니다. 필요에 따라 `FriendlyOverloads` 설정을 조정하여 기존 코드와의 호환성을 맞춥니다.

## Risks / Trade-offs

- [Risk] 기존 수동으로 정의된 COM 인터페이스/구조체의 메모리 레이아웃과 `CsWin32`가 생성한 구조체의 마샬링 방식 차이로 인한 런타임 오류 발생 가능성
  - → Mitigation: 마이그레이션 후 핵심 기능(단축키, 장치 전환, 상태 동기화)에 대한 철저한 테스트 진행
- [Risk] `CsWin32` 도입 시 빌드 시간에 약간의 오버헤드가 추가될 수 있음
  - → Mitigation: `NativeMethods.txt`에 필요한 API만 정확히 명시하여 소스 생성 범위를 최소화함
