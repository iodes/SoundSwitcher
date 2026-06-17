## 1. 패키지 설치

- [ ] 1.1 `SoundSwitcher.csproj`에 `Serilog` NuGet 패키지 설치
- [ ] 1.2 `SoundSwitcher.csproj`에 `Serilog.Sinks.File` NuGet 패키지 설치
- [ ] 1.3 `SoundSwitcher.csproj`에 `Serilog.Sinks.Console` NuGet 패키지 설치

## 2. 로깅 초기화 및 설정

- [ ] 2.1 `App.xaml.cs`의 `OnStartup`에 Serilog 초기화 로직 작성 (콘솔 및 파일 롤링 출력, `RollingInterval.Day`)
- [ ] 2.2 앱 시작 완료 시 정보(Information) 수준의 영문 로그 작성 (`"SoundSwitcher started"`)

## 3. 예외 및 종료 처리

- [ ] 3.1 `App.xaml.cs`에 `CurrentDomain.UnhandledException` 이벤트 핸들러 추가 및 Error 수준 영문 예외 로깅 구현 (`"Unhandled exception occurred"`)
- [ ] 3.2 `App.xaml.cs`의 `OnExit`에 애플리케이션 정상 종료 영문 로그 작성 (`"SoundSwitcher exited"`) 및 `Log.CloseAndFlush()` 호출

## 4. 장치 전환 및 사용자 액션 로그 추가

- [ ] 4.1 장치 전환 비즈니스 로직(AudioEndpoint 변경 시 등)에 Serilog 템플릿 기반 영문 로그(Information) 삽입 (예: `"Switched {DeviceType} to {@Device}"`)
- [ ] 4.2 시스템 트레이 아이콘 상호작용 및 단축키(Hotkey) 이벤트 처리에 Serilog 템플릿 기반 영문 로그(Information) 삽입 (예: `"User action: {ActionName} triggered"`)
