## 1. Setup

- [x] 1.1 `Microsoft.Windows.CsWin32` NuGet 패키지 설치
- [x] 1.2 프로젝트 루트에 `NativeMethods.txt` 파일 생성 및 기존 사용 중인 Win32 API 및 COM 인터페이스 목록(예: RegisterHotKey, IMMDeviceEnumerator 등) 추가

## 2. P/Invoke 리팩터링

- [x] 2.1 기존 오디오 장치 관련 클래스 및 인터페이스에서 수동으로 작성된 COM 인터페이스, 구조체, P/Invoke 선언 제거
- [x] 2.2 기존 단축키 처리 로직에서 수동으로 작성된 P/Invoke(`RegisterHotKey`, `UnregisterHotKey`) 및 관련 상수 제거
- [x] 2.3 기타 파일(`App.xaml.cs` 등)에서 참조하는 수동 `DllImport`/`LibraryImport` 및 구조체 정의 제거

## 3. 코드 통합 및 테스트

- [x] 3.1 제거된 P/Invoke 대신 `Windows.Win32.PInvoke` 등 CsWin32가 생성한 타입과 메서드를 호출하도록 코드 변경
- [x] 3.2 필요 시 포인터 연산이나 `ref`/`out` 파라미터를 CsWin32 서명에 맞게 조정
- [x] 3.3 프로젝트 정상 빌드 확인
- [x] 3.4 런타임에 글로벌 단축키 및 디바이스 전환 기능 정상 작동 여부 검증
