# Capability: Logging

## Purpose
TBD: 애플리케이션의 런타임 이벤트, 예외, 사용자 상호작용 등을 체계적으로 기록하고 모니터링하기 위한 로깅 기능 제공.

## Requirements

### Requirement: Serilog Initialization
애플리케이션은 시작 시 콘솔 및 롤링 파일 출력이 가능한 Serilog 기반 로거를 초기화해야 합니다.

#### Scenario: App startup logging initialization
- **WHEN** 애플리케이션이 시작될 때 (`OnStartup`)
- **THEN** 파일 롤링 설정(`RollingInterval.Day`)과 콘솔 출력을 모두 갖춘 Serilog 전역 로거가 구성된다.
- **THEN** 시작 완료 메시지가 정보(Information) 수준으로 기록된다.

### Requirement: Global Exception Logging
애플리케이션 내 처리되지 않은 전역 예외가 발생할 경우 이를 가로채어 로깅해야 합니다.

#### Scenario: Unhandled exception occurs
- **WHEN** 애플리케이션 실행 중 예외가 발생하여 `CurrentDomain.UnhandledException` 이벤트가 트리거될 때
- **THEN** 해당 예외가 Error 수준으로 로그 파일 및 콘솔에 기록된다.

### Requirement: App Exit Logging
애플리케이션은 정상 종료 시 상태를 로깅하고 로거 리소스를 해제해야 합니다.

#### Scenario: App graceful exit
- **WHEN** 애플리케이션이 정상 종료될 때 (`OnExit`)
- **THEN** 종료 메시지가 정보(Information) 수준으로 기록된다.

### Requirement: Device Switching Logging
사용자가 재생 또는 녹음 장치를 전환할 때, 장치 정보가 포함된 구조화된 형태의 영문 로그를 기록해야 합니다.

#### Scenario: Device is switched
- **WHEN** 사용자가 특정 오디오 장치로 전환을 완료했을 때
- **THEN** `{DeviceType}` 및 `{@Device}` 속성 등을 포함한 Serilog 템플릿 포맷을 사용하여 Information 수준으로 영문 로그가 기록된다 (예: `"Switched {DeviceType} to {@Device}"`).

### Requirement: User Action Logging
사용자가 앱 내에서 주요 액션(단축키 사용, 설정 변경 등)을 수행할 때 영문 구조화 로그를 기록해야 합니다.

#### Scenario: User performs an action
- **WHEN** 사용자가 앱의 주요 기능이나 설정을 조작했을 때
- **THEN** 액션 정보가 포함된 Serilog 템플릿 포맷을 사용하여 Information 수준으로 영문 로그가 기록된다 (예: `"User action: {ActionName} triggered via {Trigger}"`).
