## Context

SoundSwitcher는 현재 체계화된 로깅 시스템을 가지고 있지 않아 런타임 오류 추적 및 실행 기록 확인에 한계가 있습니다. GestureWheel과 유사한 로깅 방식을 적용하여 유지보수성을 향상시키고자 합니다.

## Goals / Non-Goals

**Goals:**
- Serilog를 기반으로 콘솔 및 파일 로깅 구성
- 파일 롤링 기능 적용하여 일자별로 로그 분리
- 전역 예외 처리 로직에 Serilog 연동
- 장치 전환 및 사용자 액션에 대한 의미론적(Semantic) 영문 로깅 적용

**Non-Goals:**
- 원격 서버로의 로그 전송(예: Seq, Application Insights)
- 복잡한 구조의 JSON 로깅 형식 도입

## Decisions

- **로거 패키지 선택**: `Serilog`, `Serilog.Sinks.Console`, `Serilog.Sinks.File` 사용
  - 널리 사용되며 유연한 구조적 로깅 기능을 제공하기 때문입니다.
- **파일 롤링 주기**: `RollingInterval.Day`
  - 로그 파일이 너무 커지는 것을 방지하고 관리가 용이하게 하기 위함입니다.
- **초기화 위치**: `App.xaml.cs`의 `OnStartup`
  - 애플리케이션 시작 시 즉시 로깅을 준비하고 안전하게 해제하기 적합한 위치입니다.
- **로그 메시지 포맷팅 전략**: 영문 작성 및 Serilog Message Template 사용
  - `"Switched {DeviceType} to {@Device}"`와 같이 Serilog의 템플릿 구문을 영어로 사용하여, 텍스트 검색뿐 아니라 구조화된 데이터 추출이 가능하게 합니다.

## Risks / Trade-offs

- **[파일 I/O 오버헤드]** → Serilog의 기본 동작은 버퍼링이 지원되지 않지만 클라이언트 앱이므로 큰 병목 가능성은 적습니다. 성능 이슈 시 `Serilog.Sinks.Async` 도입 고려 가능합니다.
