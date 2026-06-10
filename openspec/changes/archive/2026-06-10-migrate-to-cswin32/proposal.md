## Why

현재 프로젝트에서 Windows API 호출을 위해 직접 작성한 P/Invoke 선언(DllImport 등)을 사용하고 있습니다. 이를 Microsoft의 공식 소스 생성기인 CsWin32로 전환하여 코드를 현대화하고자 합니다. CsWin32를 사용하면 수동으로 Win32 API 서명, 구조체, 상수를 정의할 필요가 없어 오류가 줄어들고 유지보수성이 향상되며, 최신 C# 기능(예: 포인터 및 Span 지원 등)을 더 안전하고 효율적으로 활용할 수 있습니다.

## What Changes

- **CsWin32 패키지 도입**: `Microsoft.Windows.CsWin32` NuGet 패키지 설치
- **NativeMethods.txt 구성**: 프로젝트에 필요한 Win32 API 함수 목록을 정의하는 `NativeMethods.txt` 생성
- **기존 P/Invoke 제거**: 기존에 수동으로 선언된 Win32 API 선언(`DllImport`, `LibraryImport` 등) 및 관련 구조체/열거형 제거
- **코드 마이그레이션**: 제거된 P/Invoke 호출을 `Windows.Win32.PInvoke` 네임스페이스의 생성된 코드를 사용하도록 변경

## Capabilities

### New Capabilities
- `cswin32-interop`: 프로젝트 전반의 Win32 API 상호운용성(Interop) 구조를 CsWin32 기반으로 표준화

### Modified Capabilities
<!-- 기존 기능 요구사항의 변경이 없으므로 비워둡니다. -->

## Impact

- **Affected Code**: `App.xaml.cs`, 오디오 디바이스 관련 클래스, 키보드 훅 등 직접적인 Win32 API 호출을 수행하는 모든 파일
- **Dependencies**: `Microsoft.Windows.CsWin32` 추가
- **Systems**: 기능적인 시스템 변경은 없으나 빌드 시 소스 생성기가 동작하여 컴파일 타임이 약간 증가할 수 있음
