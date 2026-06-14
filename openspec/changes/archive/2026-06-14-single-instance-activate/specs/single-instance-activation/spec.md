## 추가된 요구사항 (ADDED Requirements)

### Requirement: 다중 인스턴스 방지 및 기존 인스턴스 활성화 (Prevent multiple instances & Activate existing)
애플리케이션은 시스템 전역 Mutex를 사용하여 여러 인스턴스가 동시에 실행되는 것을 방지해야 하며, 중복 실행이 시도되었을 때 기존에 실행 중인 인스턴스의 윈도우를 활성화해야 합니다.

#### Scenario: 애플리케이션이 이미 실행 중일 때 재실행 시도
- **WHEN** 다른 인스턴스가 이미 활성화되어 있는 상태에서 사용자가 SoundSwitcher를 다시 시작하려고 시도할 때
- **THEN** 두 번째 인스턴스는 Mutex를 감지하고, IPC(EventWaitHandle)를 통해 첫 번째 인스턴스에 활성화 신호를 전송한 후 즉시 종료됩니다.
- **THEN** 첫 번째 인스턴스는 신호를 수신하고 메인 창을 화면 최상단으로 활성화하여 표시합니다.

### Requirement: 명령줄 인자를 통한 창 활성화 (Activate application window via arguments)
애플리케이션은 첫 번째 인스턴스로 시작될 때 명령줄 인자를 파싱해야 하며, `/Activate`가 제공되면 시스템 트레이에 최소화되지 않고 메인 창을 화면 최상단으로 활성화해야 합니다.

#### Scenario: /Activate 인자와 함께 최초 시작
- **WHEN** SoundSwitcher가 실행 중이 아닌 상태에서 `/Activate` 인자와 함께 시작될 때
- **THEN** 애플리케이션이 정상적으로 시작되며, 메인 창이 즉시 표시되고 활성화됩니다.
