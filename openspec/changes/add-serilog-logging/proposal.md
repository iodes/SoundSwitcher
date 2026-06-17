## Why

SoundSwitcher의 로그 추적 및 문제 해결 능력을 향상시키기 위해 구조화된 로깅 프레임워크인 Serilog를 도입합니다. 파일 기반의 롤링 로그와 콘솔 출력을 모두 지원하여 개발 시에는 콘솔을 통해 즉각적으로 로그를 확인하고, 배포 후에는 파일 로그를 통해 안정적으로 문제를 추적할 수 있도록 개선합니다. 또한, 장치 전환 및 사용자 동작을 영어로 구조화하여 기록함으로써 향후 로그 분석의 용이성을 확보합니다.

## What Changes

- Serilog 및 관련 Sinks(File, Console) NuGet 패키지 추가
- 애플리케이션 시작 시 Serilog 초기화 및 로거 구성 (콘솔 및 파일 롤링 출력)
- 전역 예외 처리(Unhandled Exception) 영역에 Serilog 적용
- 파일 로그는 날짜별 롤링 기능 적용 (`log-.txt`, `RollingInterval.Day`)
- 장치 전환 및 사용자 주요 동작에 대해 Serilog 템플릿 포맷(`{VariableName}`)을 활용한 영문 구조화 로그 추가

## Capabilities

### New Capabilities
- `logging`: Serilog를 활용한 콘솔 및 롤링 파일 로그 출력 기능 도입

### Modified Capabilities


## Impact

- `SoundSwitcher.csproj`: 패키지 의존성 추가 (Serilog, Serilog.Sinks.File, Serilog.Sinks.Console)
- `App.xaml.cs`: `OnStartup` 및 `OnExit` 에 로거 초기화 및 해제, 전역 예외 로깅 코드 추가
- 장치 전환 및 사용자 이벤트 처리 로직 전반: 구조화된 영문 로그 삽입
